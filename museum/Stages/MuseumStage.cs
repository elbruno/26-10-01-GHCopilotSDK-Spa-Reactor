// Descriptor de etapas de Museum.
// Enseña que cada paso activa banderas de SessionConfig: streaming, persona,
// facts/tool local, validacion, investigacion MCP y generacion.
namespace MuseumExhibitStudio.Stages;

public sealed record MuseumStage(
    string Id,
    string Title,
    string Prompt,
    bool Streaming = false,
    bool Persona = false,
    bool Facts = false,
    bool Validate = false,
    bool Research = false,
    bool Generate = true)
{
    // Find hace que la CLI sea reproducible: el numero selecciona una configuracion fija.
    public static MuseumStage Find(string id) => id switch
    {
        "01" => FirstSession.Create(),
        "02" => StreamingCurator.Create(),
        "03" => CuratorVoice.Create(),
        "04" => ApprovedFacts.Create(),
        "05" => ProveStructure.Create(),
        "06" => WikipediaResearch.Create(),
        "99" => Finished.Create(),
        _ => throw new ArgumentException("Etapa desconocida. Usa 01..06 o 99.")
    };
}
