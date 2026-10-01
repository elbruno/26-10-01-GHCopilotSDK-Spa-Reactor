namespace MuseumExhibitStudio.Stages;

public static class FirstSession
{
    public static MuseumStage Create() => new(
        "01",
        "Primera sesion del curador",
        "Escribe dos frases de texto de museo sobre el alunizaje del Apollo 11.");
}
