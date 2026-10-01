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
