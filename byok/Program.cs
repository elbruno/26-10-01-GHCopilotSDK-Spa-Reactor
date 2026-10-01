// BYOK con Microsoft Foundry.
// Enseña ProviderConfig: el Copilot SDK puede usar un endpoint/modelo propio
// compatible con OpenAI mientras el runtime local sigue orquestando la sesion.
// Se usa como bloque opcional para explicar endpoint, modelo y autenticacion.
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
// El endpoint se deriva del recurso y siempre apunta a la ruta OpenAI-compatible.
var endpoint = BuildEndpoint(resourceUrl);
// AzureCliCredential evita guardar tokens o claves en el repositorio de la demo.
var credential = new AzureCliCredential();

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(120));
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };

try
{
    // Preflight valida Microsoft Entra sin enviar ningun prompt al modelo.
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

    // El CopilotClient sigue siendo necesario: BYOK cambia el proveedor del modelo.
    await using var client = new CopilotClient(new CopilotClientOptions
    {
        Connection = RuntimeConnection.ForStdio("copilot"),
        WorkingDirectory = AppContext.BaseDirectory
    });
    // StartAsync arranca el runtime antes de crear una sesion con ProviderConfig.
    await client.StartAsync(cancellation.Token);

#pragma warning disable GHCP001 // BYOK bearer-token callbacks are experimental in SDK 1.0.11.
    // SessionConfig.Provider redirige el modelo a Foundry en vez del proveedor Copilot.
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        ClientName = "copilot-sdk-byok-foundry",
        Model = model,
        Provider = new ProviderConfig
        {
            Type = "openai",
            BaseUrl = endpoint.AbsoluteUri,
            WireApi = "responses",
            // BearerTokenProvider entrega tokens Entra frescos sin imprimirlos.
            BearerTokenProvider = async _ =>
            {
                var refreshed = await credential.GetTokenAsync(
                    new TokenRequestContext(["https://ai.azure.com/.default"]),
                    cancellation.Token);
                return refreshed.Token;
            }
        },
        // AvailableTools vacio muestra una sesion BYOK sin herramientas externas.
        AvailableTools = [],
        Tools = [],
        EnableConfigDiscovery = false,
        EnableSkills = false,
        EnableHostGitOperations = false
    }, cancellation.Token);
#pragma warning restore GHCP001

    // SendAndWaitAsync demuestra que el flujo de prompt/respuesta no cambia con BYOK.
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
    // La configuracion privada vive fuera del codigo y se valida de forma explicita.
    var value = Environment.GetEnvironmentVariable(name)?.Trim();
    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException($"Falta la variable de entorno {name}.")
        : value;
}

static Uri BuildEndpoint(string resourceUrl)
{
    // Normaliza la URL para no aceptar endpoints inseguros o mal formados.
    if (!Uri.TryCreate(resourceUrl, UriKind.Absolute, out var resource) ||
        resource.Scheme != Uri.UriSchemeHttps ||
        string.IsNullOrWhiteSpace(resource.Host))
    {
        throw new ArgumentException(
            "FOUNDRY_RESOURCE_URL debe ser una URL HTTPS del recurso Microsoft Foundry.");
    }

    return new Uri($"{resource.GetLeftPart(UriPartial.Authority).TrimEnd('/')}/openai/v1/");
}
