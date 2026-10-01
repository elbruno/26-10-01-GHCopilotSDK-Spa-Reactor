namespace MuseumExhibitStudio.Stages;

public static class WikipediaResearch
{
    public static MuseumStage Create() => new(
        "06",
        "MCP: investigacion separada en Wikipedia",
        MuseumPrompts.Research,
        Streaming: true,
        Research: true,
        Generate: false);
}
