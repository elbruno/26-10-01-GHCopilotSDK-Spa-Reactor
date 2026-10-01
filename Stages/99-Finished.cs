namespace AccessibilityDemo.Stages;

public static class Finished
{
    public static DemoStage Create() => new("99", "Aplicacion completa: Accessibility Reviewer",
        McpReview.ReviewPrompt + """

        Consulta accessibility_rule_lookup con criterion 4.1.2 para el textbox sin nombre.
        Separa evidencia, recomendacion basada en la regla y revision manual pendiente.
        """,
        Streaming: true, Persona: true, Rules: true, Browser: true, Denial: true);
}
