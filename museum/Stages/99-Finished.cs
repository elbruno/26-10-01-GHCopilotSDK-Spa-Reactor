// Etapa Museum 99 — Aplicacion completa.
// Combina streaming, system prompt, tool local, validacion y MCP separado como
// estado de recuperacion para el directo.
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
