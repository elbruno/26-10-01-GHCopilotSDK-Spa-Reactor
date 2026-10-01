// Etapa Museum 02 — Streaming.
// Enseña como SessionConfig.Streaming permite mostrar la narracion por deltas
// mientras el asistente escribe para el publico.
namespace MuseumExhibitStudio.Stages;

public static class StreamingCurator
{
    public static MuseumStage Create() => new(
        "02",
        "Streaming del curador",
        "Escribe unas 120 palabras de texto de museo sobre el alunizaje del Apollo 11.",
        Streaming: true);
}
