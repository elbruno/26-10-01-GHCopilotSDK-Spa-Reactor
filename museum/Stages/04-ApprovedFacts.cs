// Etapa Museum 04 — Hechos aprobados.
// Enseña Tools y AvailableTools con approved_fact_lookup: el modelo debe pedir
// datos internos aprobados antes de redactar.
namespace MuseumExhibitStudio.Stages;

public static class ApprovedFacts
{
    public static MuseumStage Create() => new(
        "04",
        "Tool local: hechos aprobados",
        MuseumPrompts.Exhibit,
        Streaming: true,
        Persona: true,
        Facts: true);
}
