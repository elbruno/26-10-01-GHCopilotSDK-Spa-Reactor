using System.Text.Json;
using GitHub.Copilot;
using GitHub.Copilot.Rpc;

namespace MuseumExhibitStudio.Helpers;

#pragma warning disable GHCP001 // Permission decision variants are experimental in SDK 1.0.11.
public static class MuseumSelfTests
{
    public static void Run()
    {
        var count = 0;
        void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException($"FAIL: {description}");
            count++;
        }

        Check(CuratorFacts.BoundFacts(CuratorFacts.Apollo11Facts).Length == 5, "approved facts");
        try
        {
            CuratorFacts.BoundFacts([]);
            throw new InvalidOperationException("Empty facts accepted.");
        }
        catch (ArgumentException)
        {
            count++;
        }

        const string valid = """
            # Apollo 11: un salto compartido
            ## Narrative
            Apollo 11 despego el 16 de julio de 1969 con tres astronautas rumbo a la Luna.
            Neil Armstrong y Buzz Aldrin descendieron hasta la superficie, mientras Michael
            Collins permanecio en orbita lunar. El 20 de julio, Armstrong y Aldrin caminaron
            sobre la Luna y participaron en una mision seguida desde la Tierra. La tripulacion
            emprendio despues el regreso y llego a nuestro planeta el 24 de julio. Estos hechos
            aprobados permiten presentar la mision con una estructura clara y prudente. La
            exhibicion invita a observar el trabajo coordinado entre quienes descendieron,
            quien permanecio en orbita y los equipos que apoyaron el viaje desde la Tierra.
            ## Visitor questions
            1. Que aspecto de la mision te parece mas arriesgado?
            2. Como imaginas la experiencia de permanecer en orbita?
            3. Que pregunta harias a la tripulacion?
            """;
        Check(CuratorValidation.ValidateExhibit(valid).Valid, "valid exhibit");
        Check(!CuratorValidation.ValidateExhibit("# Incompleto").Valid, "invalid exhibit");

        var extracted = CuratorSafety.ExtractSources("""
            Contexto.
            ## Sources
            - Apollo 11: https://en.wikipedia.org/wiki/Apollo_11
            """);
        Check(extracted.Sources.Count == 1, "extract source");

        var handler = CuratorSafety.WikipediaPermissionHandler();
        var allowed = new PermissionRequestMcp
        {
            ServerName = "wikipedia",
            ToolName = "search",
            ToolTitle = "Search",
            ReadOnly = true,
            Args = JsonSerializer.SerializeToElement(new { query = "Apollo 11" })
        };
        Check(
            handler(allowed, null!).GetAwaiter().GetResult() is PermissionDecisionApproveOnce,
            "allow wikipedia");

        var denied = new PermissionRequestMcp
        {
            ServerName = "wikipedia",
            ToolName = "unknown",
            ToolTitle = "Unknown",
            ReadOnly = true
        };
        Check(
            handler(denied, null!).GetAwaiter().GetResult() is PermissionDecisionReject,
            "deny unknown");

        Console.WriteLine($"[self-test] PASS {count} checks; sin llamadas al modelo.");
    }
}
#pragma warning restore GHCP001
