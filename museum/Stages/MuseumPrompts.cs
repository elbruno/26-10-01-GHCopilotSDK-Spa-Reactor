// Prompts de Museum.
// Enseñan como escribir instrucciones de usuario que fuerzan uso de tool,
// estructura estable y separacion entre generacion e investigacion MCP.
namespace MuseumExhibitStudio.Stages;

public static class MuseumPrompts
{
    // Exhibit pide primero approved_fact_lookup para anclar la salida en datos del host.
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

    // Research limita la tarea a notas para humanos y exige fuentes estructuradas.
    public const string Research = """
        Investiga contexto sobre Apollo 11 usando solamente las tools de Wikipedia configuradas.
        Busca primero y lee como maximo dos articulos relevantes.
        Resume el contexto para un curador humano; no escribas el texto de la exhibicion.
        Termina con una seccion ## Sources y una linea por fuente:
        - <titulo>: <URL canonica de Wikipedia>
        """;
}
