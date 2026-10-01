namespace AccessibilityDemo.Stages;

public static class LocalTool
{
    public static DemoStage Create() => new("04", "Tool local",
        "Invoca accessibility_rule_lookup con criterion 4.1.2. A partir del resultado " +
        "explica el problema de un input sin nombre accesible y como asociar un label. " +
        "Es un ejemplo hipotetico; no has inspeccionado una pagina.",
        Streaming: true, Persona: true, Rules: true);
}
