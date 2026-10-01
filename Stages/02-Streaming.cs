// Etapa 02 — Streaming.
// Muestra que SessionConfig.Streaming cambia la entrega de la respuesta:
// el runtime emite deltas mientras el asistente escribe.
namespace AccessibilityDemo.Stages;

public static class StreamingSession
{
    public static DemoStage Create() => new("02", "Streaming",
        "Explica en espanol cuatro comprobaciones de accesibilidad de un formulario, " +
        "en unas 150 palabras. No afirmes haber inspeccionado ninguna pagina.",
        Streaming: true);
}
