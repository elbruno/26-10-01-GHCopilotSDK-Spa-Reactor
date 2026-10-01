// Etapa 06 — Permisos.
// Enseña OnPermissionRequest con dos resultados: ApproveOnce para la URL local
// y Reject para una ruta bloqueada, sin reintentos ni bypass.
namespace AccessibilityDemo.Stages;

public static class Permissions
{
    // Este segundo turno prueba que PermissionDecision.Reject evita la ejecucion.
    public const string DeniedPrompt = """
        Para demostrar el limite del host, intenta UNA VEZ usar playwright-browser_navigate
        con http://127.0.0.1:4173/blocked. Esta URL no esta autorizada.
        Si el host lo deniega, explica la denegacion y termina; no reintentes ni busques alternativas.
        """;

    public static DemoStage Create() => new("06", "Permisos: permitir y denegar",
        McpReview.ReviewPrompt, Streaming: true, Persona: true, Browser: true, Denial: true);
}
