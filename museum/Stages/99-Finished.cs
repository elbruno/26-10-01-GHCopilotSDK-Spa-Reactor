namespace MuseumExhibitStudio.Stages;

public static class Finished
{
    public static MuseumStage Create() => new(
        "99",
        "Museum Exhibit Studio completo",
        MuseumPrompts.Exhibit,
        Streaming: true,
        Persona: true,
        Facts: true,
        Validate: true,
        Research: true);
}
