// Etapa Museum 01 — Primera sesion.
// Enseña el minimo de Copilot SDK aplicado a una app: prompt, sesion y respuesta
// completa del curador sin streaming, tools ni persona especializada.
namespace MuseumExhibitStudio.Stages;

public static class FirstSession
{
    public static MuseumStage Create() => new(
        "01",
        "Primera sesion del curador",
        "Escribe dos frases de texto de museo sobre el alunizaje del Apollo 11.");
}
