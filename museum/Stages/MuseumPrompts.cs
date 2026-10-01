namespace MuseumExhibitStudio.Stages;

public static class MuseumPrompts
{
    public const string Exhibit = """
        Crea texto para visitantes sobre el tema aprobado por esta aplicacion.

        Invoca approved_fact_lookup primero. Usa solamente los hechos que devuelve.

        Devuelve exactamente esta estructura:

        # <titulo atractivo>
        ## Narrative
        <100-140 palabras>
        ## Visitor questions
        1. <pregunta>
        2. <pregunta>
        3. <pregunta>

        No agregues prefacio, conclusion ni hechos externos.
        """;

    public const string Research = """
        Investiga contexto sobre Apollo 11 usando solamente las tools de Wikipedia configuradas.
        Busca primero y lee como maximo dos articulos relevantes.
        Resume el contexto para un curador humano; no escribas el texto de la exhibicion.
        Termina con una seccion ## Sources y una linea por fuente:
        - <titulo>: <URL canonica de Wikipedia>
        """;
}
