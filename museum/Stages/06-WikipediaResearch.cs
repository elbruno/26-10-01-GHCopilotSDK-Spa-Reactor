// Etapa Museum 06 — Investigacion Wikipedia.
// Enseña McpServers en una sesion separada: Wikipedia aporta contexto no
// confiable para un humano, sin mezclarse con los hechos aprobados.
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
