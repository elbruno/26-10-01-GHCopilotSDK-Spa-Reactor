using GitHub.Copilot;

await using var client = new CopilotClient(new CopilotClientOptions
{
    Connection = RuntimeConnection.ForStdio("copilot"),
    WorkingDirectory = AppContext.BaseDirectory
});
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    EnableConfigDiscovery = false,
    EnableSkills = false,
    EnableHostGitOperations = false
});

var response = await session.SendAndWaitAsync(
    new MessageOptions
    {
        Prompt = "Explica en una frase qué aporta GitHub Copilot SDK " +
            "para programadores de C#."
    },
    TimeSpan.FromSeconds(90));

Console.WriteLine(response?.Data.Content ?? "La sesión terminó sin respuesta.");
