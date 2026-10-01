namespace AccessibilityDemo.Stages;

public static class FirstSession
{
    public static DemoStage Create() => new("01", "Primera sesion",
        "Explica en espanol, en tres frases, que revisaria un asistente de accesibilidad " +
        "en una pagina con un formulario. No afirmes que la has inspeccionado.");
}
