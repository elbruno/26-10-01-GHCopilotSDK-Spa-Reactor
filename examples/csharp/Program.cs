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

const string prompt = "Explica en una frase qué aporta GitHub Copilot SDK " +
    "para programadores de C#.";

Console.WriteLine("Pregunta:");
Console.WriteLine(prompt);
Console.WriteLine();
Console.WriteLine("Respuesta:");

var response = await session.SendAndWaitAsync(
    new MessageOptions
    {
        Prompt = prompt
    },
    TimeSpan.FromSeconds(90));

Console.WriteLine(response?.Data.Content ?? "La sesión terminó sin respuesta.");
