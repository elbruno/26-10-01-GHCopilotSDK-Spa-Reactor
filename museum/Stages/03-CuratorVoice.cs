// Etapa Museum 03 — Voz del curador.
// Enseña SystemMessageConfig: el host reemplaza la persona generica por una voz
// interpretativa y limites editoriales propios de la aplicacion.
namespace MuseumExhibitStudio.Stages;

public static class CuratorVoice
{
    public static MuseumStage Create() => new(
        "03",
        "System prompt: voz del curador",
        "Escribe dos frases de texto para visitantes sobre el alunizaje del Apollo 11.",
        Streaming: true,
        Persona: true);
}
