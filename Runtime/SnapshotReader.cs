// Lector de snapshots de Accessibility.
// Enseña una tool local que devuelve evidencia producida por Playwright MCP,
// pero solo si el archivo pertenece a esta corrida, es pequeno y no es enlace.
// Existe porque el host no confia en rutas sugeridas por el modelo.
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
        // La tool rechaza directorios inexistentes o reparse points antes de leer.
        if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("No hay directorio de snapshots seguro para esta ejecucion.");

        // Cada snapshot se consume una sola vez para evitar reutilizar evidencia vieja.
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

    // DefineTool publica la lectura como capacidad del SDK, no como acceso libre a disco.
    public AIFunction Create() => CopilotTool.DefineTool(
        () => Task.FromResult(Read()),
        // SkipPermission=false permite que DemoPolicy apruebe la lectura por etapa.
        toolOptions: new CopilotToolOptions { SkipPermission = false },
        factoryOptions: new AIFunctionFactoryOptions
        {
            Name = "read_latest_accessibility_snapshot",
            Description = "Reads one new, bounded Playwright accessibility snapshot from this run only."
        });
}
