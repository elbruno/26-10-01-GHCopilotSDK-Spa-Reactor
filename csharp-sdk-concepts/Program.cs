using GitHub.Copilot;
using CsharpSdkConcepts;

if (args is ["--self-test"])
{
    ConceptSelfTests.Run();
    return;
}

if (args is not ["--preflight"] &&
    args is not ["--stage", _, "--model", _] &&
    args is not ["--stage", "07", "--model", _, "--question", _])
{
    Console.Error.WriteLine(
        "Uso: --preflight | --self-test | --stage 01..07 --model ID [--question conteo|categorias|prioridades]");
    Environment.ExitCode = 2;
    return;
}

using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

try
{
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
        Console.WriteLine("[runtime] conectado; modelos disponibles:");
        foreach (var model in models) Console.WriteLine(model.Id);
        Console.WriteLine($"[workiq] plugin configurado={WorkIqSafety.PluginDirectoryIsConfigured()}");
        return;
    }

    var stage = ConceptStage.Find(args[1]);
    var modelId = args[3];
    if (!models.Any(model => model.Id == modelId))
        throw new ArgumentException("Modelo no disponible. Consulta --preflight.");

    var question = args.Length == 6 ? args[5] : "prioridades";
    Console.WriteLine($"=== {stage.Id}: {stage.Title} | {modelId} ===");

    await using var session = await client.CreateSessionAsync(
        ConceptSessionFactory.Create(stage, modelId),
        cancellation.Token);

    var prompt = stage.Id == "07"
        ? WorkIqSafety.BuildPrompt(question)
        : stage.Prompt;

    Console.WriteLine("Pregunta:");
    Console.WriteLine(stage.Id == "07" ? WorkIqSafety.DisplayQuestion(question) : prompt);
    Console.WriteLine();
    Console.WriteLine("Respuesta:");

    var response = await ConceptStreamer.RunAsync(
        session,
        prompt,
        printContent: stage.Id != "07",
        cancellation.Token);

    if (stage.Id == "07")
    {
        var safeSummary = WorkIqSafety.ParseAndValidate(response);
        WorkIqSafety.Print(safeSummary);
    }

    Console.WriteLine();
    Console.WriteLine($"[verified] stage={stage.Id}");
}
catch (Exception error) when (error is InvalidOperationException or ArgumentException or
    IOException or TimeoutException or OperationCanceledException or System.Text.Json.JsonException)
{
    Console.Error.WriteLine($"[ERROR] {error.GetType().Name}: {error.Message}");
    Environment.ExitCode = 1;
}
