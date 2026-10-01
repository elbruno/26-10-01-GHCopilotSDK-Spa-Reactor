// Etapa 01 — Primera sesion.
// Enseña el caso minimo: una sesion del SDK recibe un prompt y devuelve una
// respuesta completa, sin streaming, system prompt, tools ni MCP.
namespace AccessibilityDemo.Stages;

public static class FirstSession
{
    public static DemoStage Create() => new("01", "Primera sesion",
        "Explica en espanol, en tres frases, que revisaria un asistente de accesibilidad " +
        "en una pagina con un formulario. No afirmes que la has inspeccionado.");
}
