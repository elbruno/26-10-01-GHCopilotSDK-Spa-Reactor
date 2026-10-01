namespace CsharpSdkConcepts;

public sealed record ConceptStage(string Id, string Title, string Prompt)
{
    private static readonly IReadOnlyDictionary<string, ConceptStage> Stages =
        new[]
        {
            new ConceptStage(
                "01",
                "Streaming",
                "Explica en tres frases breves por que una aplicacion puede querer streaming al usar Copilot SDK."),
            new ConceptStage(
                "02",
                "System prompt",
                "Explica que es una sesion del Copilot SDK."),
            new ConceptStage(
                "03",
                "Tool local",
                "Usa sdk_concept_lookup para explicar el concepto streaming. No respondas sin invocar la tool."),
            new ConceptStage(
                "04",
                "Permisos",
                "Usa sdk_concept_lookup para explicar el concepto permisos. No respondas sin invocar la tool."),
            new ConceptStage(
                "05",
                "MCP",
                "Usa Wikipedia para buscar Apollo 11 y responde con un unico dato historico verificable."),
            new ConceptStage(
                "06",
                "Skill local",
                "Explica como se combinan host, sesion, prompt y eventos en GitHub Copilot SDK."),
            new ConceptStage(
                "07",
                "WorkIQ seguro",
                string.Empty)
        }.ToDictionary(stage => stage.Id, StringComparer.OrdinalIgnoreCase);

    public static ConceptStage Find(string id) =>
        Stages.TryGetValue(id, out var stage)
            ? stage
            : throw new ArgumentException("Etapa desconocida. Usa 01..07.");
}
