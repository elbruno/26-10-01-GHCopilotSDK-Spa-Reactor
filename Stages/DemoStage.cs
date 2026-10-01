// Descriptor de etapas de Accessibility.
// Enseña que la demo no cambia codigo entre pasos: cada bandera activa piezas
// de SessionConfig como Streaming, SystemMessage, Tools, McpServers y permisos.
namespace AccessibilityDemo.Stages;

public sealed record DemoStage(
    string Id, string Title, string Prompt,
    bool Streaming = false, bool Persona = false, bool Rules = false,
    bool Browser = false, bool Denial = false)
{
    // Find centraliza el enrutamiento CLI para que cada etapa sea reproducible.
    public static DemoStage Find(string id) => id switch
    {
        "01" => FirstSession.Create(),
        "02" => StreamingSession.Create(),
        "03" => SystemPrompt.Create(),
        "04" => LocalTool.Create(),
        "05" => McpReview.Create(),
        "06" => Permissions.Create(),
        "99" => Finished.Create(),
        _ => throw new ArgumentException("Etapa desconocida. Usa 01, 02, 03, 04, 05, 06 o 99.")
    };
}
