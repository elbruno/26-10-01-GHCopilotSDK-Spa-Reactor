using AccessibilityDemo.Runtime;
using AccessibilityDemo.Stages;
using GitHub.Copilot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

if (args is ["--serve"])
{
    var appBuilder = WebApplication.CreateBuilder();
    appBuilder.Logging.ClearProviders();
    var app = appBuilder.Build();
    var page = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "page", "index.html"));
    app.MapGet("/", () => Results.Content(page, "text/html; charset=utf-8"));
    Console.WriteLine("Pagina controlada: http://127.0.0.1:4173/ (Ctrl+C para detener)");
    await app.RunAsync("http://127.0.0.1:4173");
    return;
}

if (args is ["--self-test"])
{
    SelfTests.Run();
    return;
}

if (args is not ["--preflight"] &&
    args is not ["--review", "--model", _] &&
    args is not ["--stage", _, "--model", _])
{
    Console.Error.WriteLine("Uso: --preflight | --self-test | --serve | --review --model ID | --stage 01..06/99 --model ID");
    Environment.ExitCode = 2;
    return;
}

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(180));
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
try
{
    // Explicit CLI path keeps the rehearsal on the installed runtime, not an implicit download.
    await using var client = new CopilotClient(new CopilotClientOptions
    {
        Connection = RuntimeConnection.ForStdio("copilot"),
        WorkingDirectory = AppContext.BaseDirectory
    });
    await client.StartAsync(cancellation.Token);
    var auth = await client.GetAuthStatusAsync(cancellation.Token);
    Console.WriteLine($"[auth] authenticated={auth.IsAuthenticated}");
    if (!auth.IsAuthenticated)
        throw new InvalidOperationException("Falta autenticacion. Ejecuta copilot login fuera de camara.");

    var models = await client.ListModelsAsync(cancellation.Token);
    if (args is ["--preflight"])
    {
        Console.WriteLine("[runtime] conectado; modelos disponibles (sin datos de cuenta):");
        foreach (var model in models) Console.WriteLine(model.Id);
        return;
    }

    var stage = args[0] == "--review" ? Finished.Create() : DemoStage.Find(args[1]);
    var modelId = args[0] == "--review" ? args[2] : args[3];
    if (!models.Any(m => m.Id == modelId))
        throw new ArgumentException("Modelo no disponible. Consulta --preflight.");

    Console.WriteLine($"=== {stage.Id}: {stage.Title} | {modelId} ===");
    var runDirectory = Path.Combine(AppContext.BaseDirectory, ".runs", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(runDirectory);
    var policy = new DemoPolicy(stage.Rules, stage.Browser);
    var snapshots = new SnapshotReader(runDirectory);
    var ruleCalls = 0;
    List<AIFunctionDeclaration> tools = [];
    List<string> names = [];
    if (stage.Rules)
    {
        tools.Add(RuleCatalog.Create(() => ruleCalls++));
        names.Add("accessibility_rule_lookup");
    }
    if (stage.Browser)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var response = await http.GetAsync(DemoPolicy.Target, cancellation.Token);
        response.EnsureSuccessStatusCode();
        tools.Add(snapshots.Create());
        names.Add("read_latest_accessibility_snapshot");
        names.Add("playwright-browser_navigate");
    }

    var config = new SessionConfig
    {
        Model = modelId,
        Streaming = stage.Streaming,
        AvailableTools = names,
        Tools = tools,
        OnPermissionRequest = policy.Decide,
        WorkingDirectory = runDirectory,
        EnableConfigDiscovery = false,
        EnableSkills = false,
        EnableHostGitOperations = false,
        SystemMessage = stage.Persona
            ? new SystemMessageConfig { Mode = SystemMessageMode.Replace, Content = SystemPrompt.Instructions }
            : null
    };
    if (stage.Browser)
    {
        var server = Path.Combine(AppContext.BaseDirectory, "..", "..", "..",
            "node_modules", "@playwright", "mcp", "cli.js");
        if (!File.Exists(server))
            throw new FileNotFoundException("Falta Playwright MCP. Ejecuta npm ci en la raiz del repositorio.", server);

        config.McpServers = new Dictionary<string, McpServerConfig>
        {
            ["playwright"] = new McpStdioServerConfig
            {
                Command = "node",
                Args = [Path.GetFullPath(server), "--browser=msedge", "--isolated", "--headless",
                    "--allowed-origins", DemoPolicy.Target.GetLeftPart(UriPartial.Authority),
                    "--block-service-workers", "--output-dir", runDirectory, "--output-mode", "file"],
                WorkingDirectory = runDirectory,
                Tools = ["browser_navigate"]
            }
        };
    }

    await using var session = await client.CreateSessionAsync(config, cancellation.Token);
    var deltas = 0;
    var successfulTools = 0;
    var sessionErrors = 0;
    using var subscription = session.On<SessionEvent>(e =>
    {
        switch (e)
        {
            case AssistantMessageDeltaEvent delta when stage.Streaming:
                if (!string.IsNullOrEmpty(delta.Data.DeltaContent))
                {
                    deltas++;
                    Console.Write(delta.Data.DeltaContent);
                }
                break;
            case ToolExecutionStartEvent tool:
                Console.WriteLine($"\n[tool:start] {tool.Data.ToolName}");
                break;
            case ToolExecutionCompleteEvent tool:
                if (tool.Data.Success) successfulTools++;
                Console.WriteLine($"\n[tool:done] success={tool.Data.Success}");
                break;
            case SessionErrorEvent error:
                sessionErrors++;
                Console.Error.WriteLine($"\n[session:error] {error.Data.Message}");
                break;
        }
    });

    async Task Turn(string prompt)
    {
        var result = await session.SendAndWaitAsync(new MessageOptions { Prompt = prompt },
            TimeSpan.FromSeconds(90), cancellation.Token);
        if (sessionErrors != 0)
            throw new InvalidOperationException("La sesion emitio un error; no se considera un ensayo exitoso.");
        if (result is null || string.IsNullOrWhiteSpace(result.Data.Content))
            throw new InvalidOperationException("El turno termino sin respuesta del asistente.");
        if (!stage.Streaming) Console.WriteLine(result.Data.Content);
        Console.WriteLine();
    }

    await Turn(stage.Prompt);
    if (!stage.Rules && !stage.Browser && successfulTools != 0)
        throw new InvalidOperationException("Se ejecuto una tool inesperada en una etapa sin capacidades.");
    if (stage.Streaming && deltas == 0)
        throw new InvalidOperationException("No se recibieron deltas: streaming no demostrado.");
    if (stage.Rules && ruleCalls == 0)
        throw new InvalidOperationException("El modelo no ejecuto el handler de reglas.");
    if (stage.Browser && (policy.ApprovedNavigations == 0 || snapshots.Reads == 0 || successfulTools < 2))
        throw new InvalidOperationException("No se demostro navegacion autorizada y lectura real del snapshot.");

    if (stage.Denial)
    {
        var before = successfulTools;
        await Turn(Permissions.DeniedPrompt);
        if (policy.RejectedNavigations == 0 || successfulTools != before)
            throw new InvalidOperationException("No se demostro una denegacion sin ejecucion exitosa.");
    }
    Console.WriteLine($"[verified] stage={stage.Id} deltas={deltas} rules={ruleCalls} " +
        $"snapshots={snapshots.Reads} navigation-allowed={policy.ApprovedNavigations} " +
        $"navigation-denied={policy.RejectedNavigations}");
}
catch (Exception error) when (error is InvalidOperationException or ArgumentException or
    IOException or HttpRequestException or TimeoutException or OperationCanceledException)
{
    Console.Error.WriteLine($"[ERROR] {error.GetType().Name}: {error.Message}");
    Environment.ExitCode = 1;
}
