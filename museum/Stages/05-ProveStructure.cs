namespace MuseumExhibitStudio.Stages;

public static class ProveStructure
{
    public static MuseumStage Create() => new(
        "05",
        "Validacion determinista",
        MuseumPrompts.Exhibit,
        Streaming: true,
        Persona: true,
        Facts: true,
        Validate: true);
}
