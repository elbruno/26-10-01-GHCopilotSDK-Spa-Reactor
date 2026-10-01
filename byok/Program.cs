using Azure.Core;
using Azure.Identity;
using GitHub.Copilot;

if (args is not ["--preflight"] && args is not ["--run"])
{
    Console.Error.WriteLine("Uso: --preflight | --run");
    Environment.ExitCode = 2;
    return;
}

var resourceUrl = RequiredEnvironment("FOUNDRY_RESOURCE_URL");
var model = RequiredEnvironment("FOUNDRY_MODEL");
var endpoint = BuildEndpoint(resourceUrl);
var credential = new AzureCliCredential();

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(120));
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };

try
{
    var token = await credential.GetTokenAsync(
        new TokenRequestContext(["https://ai.azure.com/.default"]),
        cancellation.Token);
    if (string.IsNullOrWhiteSpace(token.Token))
        throw new InvalidOperationException("Azure CLI no devolvio un token de Microsoft Entra.");

    Console.WriteLine("[entra] Azure CLI credential disponible; el token no se muestra.");
    Console.WriteLine($"[provider] Microsoft Foundry OpenAI-compatible | model={model}");

    if (args is ["--preflight"])
    {
        Console.WriteLine("[preflight] PASS; no se llamo al modelo.");
        return;
    }

    await using var client = new CopilotClient(new CopilotClientOptions
    {
        Connection = RuntimeConnection.ForStdio("copilot"),
        WorkingDirectory = AppContext.BaseDirectory
    });
    await client.StartAsync(cancellation.Token);

#pragma warning disable GHCP001 // BYOK bearer-token callbacks are experimental in SDK 1.0.11.
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        ClientName = "copilot-sdk-byok-foundry",
        Model = model,
        Provider = new ProviderConfig
        {
            Type = "openai",
            BaseUrl = endpoint.AbsoluteUri,
            WireApi = "responses",
            BearerTokenProvider = async _ =>
            {
                var refreshed = await credential.GetTokenAsync(
                    new TokenRequestContext(["https://ai.azure.com/.default"]),
                    cancellation.Token);
                return refreshed.Token;
            }
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
            Prompt = """
                En este ejemplo, BYOK significa configurar un proveedor de modelos propio
                mediante ProviderConfig; no significa claves de cifrado administradas por el cliente.
                Explica en espanol y en dos frases que demuestra esta aplicacion.
                """
        },
        TimeSpan.FromSeconds(90),
        cancellation.Token);

    var content = response?.Data.Content;
    if (string.IsNullOrWhiteSpace(content))
        throw new InvalidOperationException("El proveedor BYOK termino sin respuesta.");

    Console.WriteLine(content);
    Console.WriteLine("[verified] provider=foundry auth=entra-via-azure-cli github-copilot-auth=not-used");
}
catch (Exception error) when (error is AuthenticationFailedException or InvalidOperationException or
    ArgumentException or IOException or TimeoutException or OperationCanceledException)
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
