namespace AccessibilityDemo.Stages;

public static class McpReview
{
    public const string ReviewPrompt = """
        Invoca playwright-browser_navigate para abrir http://127.0.0.1:4173/.
        Despues invoca read_latest_accessibility_snapshot para leer la evidencia de ESTE turno.
        Informa el titulo y hasta tres observaciones verificables del snapshot.
        Un snapshot no mide contraste ni prueba el teclado ni certifica WCAG.
        No uses otras URLs, archivos ni tools.
        """;

    public static DemoStage Create() => new("05", "MCP con pagina controlada",
        ReviewPrompt, Streaming: true, Persona: true, Browser: true);
}
