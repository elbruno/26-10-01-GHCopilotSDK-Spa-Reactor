namespace AccessibilityDemo.Stages;

public static class Permissions
{
    public const string DeniedPrompt = """
        Para demostrar el limite del host, intenta UNA VEZ usar playwright-browser_navigate
        con http://127.0.0.1:4173/blocked. Esta URL no esta autorizada.
        Si el host lo deniega, explica la denegacion y termina; no reintentes ni busques alternativas.
        """;

    public static DemoStage Create() => new("06", "Permisos: permitir y denegar",
        McpReview.ReviewPrompt, Streaming: true, Persona: true, Browser: true, Denial: true);
}
