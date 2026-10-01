using System.Text.Json;
using System.Text.Json.Serialization;

namespace CsharpSdkConcepts;

public sealed record WorkIqCategory(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("count")] int Count);

public sealed record WorkIqSummary(
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("categories")] IReadOnlyList<WorkIqCategory> Categories,
    [property: JsonPropertyName("urgentCount")] int UrgentCount);

public static class WorkIqSafety
{
    private static readonly HashSet<string> AllowedCategories =
    [
        "reuniones",
        "seguimiento",
        "documentos",
        "administrativo",
        "otro"
    ];

    public const string SystemMessage = """
        Eres un filtro de privacidad para una demo publica.
        Usa exclusivamente workiq-ask y nunca uses tools de escritura.
        Analiza solamente correos de las ultimas 24 horas.
        No incluyas nombres, direcciones, dominios, asuntos, citas, fragmentos, URLs,
        nombres de empresas, proyectos ni ningun texto procedente de un mensaje.
        Devuelve solamente JSON sin Markdown con este contrato exacto:
        {"totalItems":0,"categories":[{"name":"reuniones","count":0}],"urgentCount":0}
        Los unicos nombres de categoria permitidos son reuniones, seguimiento,
        documentos, administrativo y otro. Los valores deben ser enteros entre 0 y 20.
        """;

    public static bool PluginDirectoryIsConfigured()
    {
        var path = Environment.GetEnvironmentVariable("WORKIQ_PLUGIN_DIR");
        return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
    }

    public static string GetPluginDirectory()
    {
        var path = Environment.GetEnvironmentVariable("WORKIQ_PLUGIN_DIR");
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                "Define WORKIQ_PLUGIN_DIR con la ruta absoluta del plugin WorkIQ instalado.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath) ||
            !File.Exists(Path.Combine(fullPath, ".mcp.json")) ||
            !Directory.Exists(Path.Combine(fullPath, "skills")))
        {
            throw new InvalidOperationException(
                "WORKIQ_PLUGIN_DIR no apunta a un plugin WorkIQ valido.");
        }

        return fullPath;
    }

    public static string DisplayQuestion(string question) => NormalizeQuestion(question) switch
    {
        "conteo" => "¿Cuantos temas de correo requieren atencion en las ultimas 24 horas?",
        "categorias" => "¿En que categorias generales se agrupan mis temas recientes?",
        _ => "¿Cuantos temas urgentes y de seguimiento requieren atencion?"
    };

    public static string BuildPrompt(string question) => $"""
        Invoca workiq-ask para responder esta pregunta de forma agregada:
        {DisplayQuestion(question)}

        Sigue el contrato JSON y las restricciones de privacidad del system prompt.
        No muestres ni repitas ningun dato original.
        """;

    public static WorkIqSummary ParseAndValidate(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLineEnd < 0 || lastFence <= firstLineEnd)
                throw new InvalidOperationException("WorkIQ no devolvio JSON valido.");
            trimmed = trimmed[(firstLineEnd + 1)..lastFence].Trim();
        }

        using var document = JsonDocument.Parse(trimmed);
        var root = document.RootElement;
        var allowedFields = new HashSet<string>(StringComparer.Ordinal)
        {
            "totalItems", "categories", "urgentCount"
        };
        if (root.ValueKind != JsonValueKind.Object ||
            root.EnumerateObject().Any(property => !allowedFields.Contains(property.Name)))
        {
            throw new InvalidOperationException("La respuesta contiene campos no permitidos.");
        }

        var summary = JsonSerializer.Deserialize<WorkIqSummary>(trimmed)
            ?? throw new InvalidOperationException("La respuesta agregada esta vacia.");
        ValidateCount(summary.TotalItems, "totalItems");
        ValidateCount(summary.UrgentCount, "urgentCount");

        if (summary.Categories.Count > AllowedCategories.Count ||
            summary.Categories.Any(category =>
                !AllowedCategories.Contains(category.Name) ||
                category.Name.Any(char.IsUpper) ||
                category.Count is < 0 or > 20))
        {
            throw new InvalidOperationException("La respuesta contiene categorias o conteos no permitidos.");
        }

        return summary;
    }

    public static void Print(WorkIqSummary summary)
    {
        Console.WriteLine($"Temas agregados: {summary.TotalItems}");
        Console.WriteLine($"Urgentes: {summary.UrgentCount}");
        foreach (var category in summary.Categories.OrderByDescending(item => item.Count))
            Console.WriteLine($"- {category.Name}: {category.Count}");
    }

    private static string NormalizeQuestion(string question) =>
        question.Trim().ToLowerInvariant() switch
        {
            "conteo" => "conteo",
            "categorias" => "categorias",
            "prioridades" => "prioridades",
            _ => throw new ArgumentException(
                "Pregunta no permitida. Usa conteo, categorias o prioridades.")
        };

    private static void ValidateCount(int value, string field)
    {
        if (value is < 0 or > 20)
            throw new InvalidOperationException($"{field} queda fuera del rango seguro.");
    }
}
