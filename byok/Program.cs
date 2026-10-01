// =============================================================================
// BYOK con Microsoft Foundry.
// -----------------------------------------------------------------------------
// Separa dos conceptos que suelen confundirse:
//
//  1. CopilotClient.ListModelsAsync() consulta el catalogo del runtime Copilot.
//     Sirve para conocer ids y capacidades que el runtime sabe orquestar.
//  2. ProviderConfig cambia donde se ejecuta la inferencia. El modelo real se
//     envia a Foundry y se autentica con Entra o con una API key.
//
// Los deployments BYOK no se descubren automaticamente con ListModelsAsync:
// la aplicacion selecciona explicitamente FOUNDRY_MODEL.
// =============================================================================

using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using GitHub.Copilot;

if (args is not ["--list-models"] &&
    args is not ["--preflight"] &&
    args is not ["--run"])
{
    Console.Error.WriteLine("Uso: --list-models | --preflight | --run");
    Environment.ExitCode = 2;
    return;
}

using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };

try
{
    await using var client = new CopilotClient(new CopilotClientOptions
    {
        Connection = RuntimeConnection.ForStdio("copilot"),
        WorkingDirectory = AppContext.BaseDirectory
    });
    await client.StartAsync(cancellation.Token);

    // Esta lista viene del runtime de Copilot, no del endpoint Foundry.
    var copilotModels = await client.ListModelsAsync(cancellation.Token);
    Console.WriteLine("[copilot-catalog] ListModelsAsync() consulta el runtime Copilot:");
    foreach (var id in copilotModels.Select(item => item.Id)
                 .Where(id => id is "gpt-6.1-sol" or "gpt-6-luna" or "grok-4.6"))
    {
        Console.WriteLine($"- {id}");
    }

    if (args is ["--list-models"])
    {
        Console.WriteLine(
            "[copilot-catalog] BYOK no descubre deployments: FOUNDRY_MODEL selecciona el modelo del proveedor.");
        return;
    }

    var resourceUrl = RequiredEnvironment("FOUNDRY_RESOURCE_URL");
    var model = RequiredEnvironment("FOUNDRY_MODEL");
    var authMode = OptionalEnvironment("FOUNDRY_AUTH_MODE", "entra").ToLowerInvariant();
    if (authMode is not ("entra" or "api-key"))
        throw new InvalidOperationException("FOUNDRY_AUTH_MODE debe ser entra o api-key.");

    var endpoint = BuildEndpoint(resourceUrl);
    var credential = authMode == "entra" ? new AzureCliCredential() : null;
    var apiKey = authMode == "api-key" ? RequiredEnvironment("FOUNDRY_API_KEY") : null;

    var listedByCopilot = copilotModels.Any(item => item.Id == model);
    Console.WriteLine($"[copilot-catalog] configured-model={model} listed={listedByCopilot}");

    // /models valida endpoint y autenticacion sin consumir una inferencia.
    var providerModels = await ListProviderModelsAsync(
        endpoint,
        credential,
        apiKey,
        cancellation.Token);
    Console.WriteLine(
        $"[foundry-catalog] authenticated={authMode} models={providerModels.Count} configured-listed={providerModels.Contains(model)}");

    if (args is ["--preflight"])
    {
        Console.WriteLine("[preflight] PASS; no se envio ningun prompt.");
        return;
    }

    var provider = new ProviderConfig
    {
        Type = "openai",
        BaseUrl = endpoint.AbsoluteUri,
        WireApi = "responses",
        MaxPromptTokens = 8000,
        MaxOutputTokens = 256,
        // ModelId decide la configuracion de agente del runtime; WireModel es el
        // nombre que recibe Foundry. En esta demo ambos nombres coinciden.
        ModelId = model,
        WireModel = model
    };

#pragma warning disable GHCP001 // BYOK auth callbacks are experimental in SDK 1.0.11.
    if (credential is not null)
    {
        provider.BearerTokenProvider = async _ =>
        {
            var refreshed = await credential.GetTokenAsync(
                new TokenRequestContext(["https://ai.azure.com/.default"]),
                cancellation.Token);
            return refreshed.Token;
        };
    }
    else
    {
        // Foundry v1 usa api-key. La key viaja al runtime, pero nunca se imprime.
        provider.Headers = new Dictionary<string, string> { ["api-key"] = apiKey! };
    }

    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        ClientName = "copilot-sdk-byok-foundry",
        Model = model,
        Provider = provider,
        SystemMessage = new SystemMessageConfig
        {
            Mode = SystemMessageMode.Replace,
            Content = """
                Eres un asistente de demostracion sin tools ni acceso a archivos.
                Responde directamente en espanol con exactamente dos frases breves.
                No describas planes, busquedas, razonamiento interno ni llamadas a tools.
                """
        },
        AvailableTools = [],
        Tools = [],
        EnableConfigDiscovery = false,
        EnableSkills = false,
        EnableHostGitOperations = false
    }, cancellation.Token);
#pragma warning restore GHCP001

    var response = await session.SendAndWaitAsync(
        new MessageOptions
        {
            Prompt = $"""
                Explica la diferencia: ListModelsAsync enumera el catalogo que conoce
                el runtime Copilot; ProviderConfig dirige la inferencia al modelo
                {model} de Microsoft Foundry.
                """
        },
        TimeSpan.FromMinutes(4),
        cancellation.Token);

    var content = response?.Data.Content;
    if (string.IsNullOrWhiteSpace(content))
        throw new InvalidOperationException("El proveedor BYOK termino sin respuesta.");

    Console.WriteLine(content);
    Console.WriteLine($"[verified] inference-provider=foundry auth={authMode} model={model}");
}
catch (Exception error) when (error is AuthenticationFailedException or InvalidOperationException or
    ArgumentException or IOException or HttpRequestException or JsonException or TimeoutException or
    OperationCanceledException)
{
    Console.Error.WriteLine($"[ERROR] {error.GetType().Name}: {error.Message}");
    Environment.ExitCode = 1;
}

static string RequiredEnvironment(string name)
{
    var value = Environment.GetEnvironmentVariable(name)?.Trim();
    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException($"Falta la variable de entorno {name}.")
        : value;
}

static string OptionalEnvironment(string name, string fallback) =>
    Environment.GetEnvironmentVariable(name)?.Trim() is { Length: > 0 } value
        ? value
        : fallback;

static Uri BuildEndpoint(string resourceUrl)
{
    if (!Uri.TryCreate(resourceUrl, UriKind.Absolute, out var resource) ||
        resource.Scheme != Uri.UriSchemeHttps ||
        string.IsNullOrWhiteSpace(resource.Host))
    {
        throw new ArgumentException(
            "FOUNDRY_RESOURCE_URL debe ser una URL HTTPS del recurso Microsoft Foundry.");
    }

    return new Uri($"{resource.GetLeftPart(UriPartial.Authority).TrimEnd('/')}/openai/v1/");
}

static async Task<HashSet<string>> ListProviderModelsAsync(
    Uri endpoint,
    TokenCredential? credential,
    string? apiKey,
    CancellationToken cancellationToken)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(endpoint, "models"));
    if (credential is not null)
    {
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(["https://ai.azure.com/.default"]),
            cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }
    else
    {
        request.Headers.Add("api-key", apiKey);
    }

    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    using var response = await http.SendAsync(request, cancellationToken);
    response.EnsureSuccessStatusCode();

    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    if (!document.RootElement.TryGetProperty("data", out var data) ||
        data.ValueKind != JsonValueKind.Array)
    {
        throw new InvalidOperationException("Foundry /models no devolvio el contrato esperado.");
    }

    return data.EnumerateArray()
        .Where(item => item.TryGetProperty("id", out _))
        .Select(item => item.GetProperty("id").GetString())
        .Where(id => !string.IsNullOrWhiteSpace(id))
        .Select(id => id!)
        .ToHashSet(StringComparer.Ordinal);
}
