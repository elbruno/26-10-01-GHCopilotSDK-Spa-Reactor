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
