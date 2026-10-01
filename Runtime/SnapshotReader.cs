using GitHub.Copilot;
using Microsoft.Extensions.AI;

namespace AccessibilityDemo.Runtime;

public sealed class SnapshotReader(string outputDirectory)
{
    private readonly HashSet<string> consumed = new(StringComparer.OrdinalIgnoreCase);
    public int Reads { get; private set; }

    public string Read()
    {
        var directory = new DirectoryInfo(outputDirectory);
        if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("No hay directorio de snapshots seguro para esta ejecucion.");

        var file = directory.EnumerateFiles("page-*.yml", SearchOption.TopDirectoryOnly)
            .Where(f => !consumed.Contains(f.FullName) &&
                        !f.Attributes.HasFlag(FileAttributes.ReparsePoint) &&
                        f.Length is > 0 and <= 1_000_000)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("No hay snapshot nuevo. Invoca browser_navigate primero.");

        var text = File.ReadAllText(file.FullName);
        consumed.Add(file.FullName);
        Reads++;
        Console.WriteLine($"\n[handler:snapshot] bytes={file.Length}");
        return text;
    }

    public AIFunction Create() => CopilotTool.DefineTool(
        () => Task.FromResult(Read()),
        toolOptions: new CopilotToolOptions { SkipPermission = false },
        factoryOptions: new AIFunctionFactoryOptions
        {
            Name = "read_latest_accessibility_snapshot",
            Description = "Reads one new, bounded Playwright accessibility snapshot from this run only."
        });
}
