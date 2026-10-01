namespace CsharpSdkConcepts;

public static class ConceptSelfTests
{
    public static void Run()
    {
        var checks = 0;
        Check(ConceptStage.Find("01").Title == "Streaming", "stage lookup", ref checks);
        Check(ConceptStage.Find("07").Title == "WorkIQ seguro", "workiq stage", ref checks);

        var safe = WorkIqSafety.ParseAndValidate(
            """{"totalItems":3,"categories":[{"name":"seguimiento","count":2}],"urgentCount":1}""");
        Check(safe.TotalItems == 3, "safe total", ref checks);
        Check(safe.Categories.Single().Name == "seguimiento", "safe category", ref checks);

        ExpectFailure(
            """{"totalItems":1,"categories":[],"urgentCount":0,"subject":"privado"}""",
            "reject extra field",
            ref checks);
        ExpectFailure(
            """{"totalItems":1,"categories":[{"name":"Proyecto secreto","count":1}],"urgentCount":0}""",
            "reject unsafe category",
            ref checks);
        ExpectFailure(
            """{"totalItems":99,"categories":[],"urgentCount":0}""",
            "reject unbounded count",
            ref checks);

        Console.WriteLine($"[self-test] PASS {checks} checks; sin llamadas al modelo ni a WorkIQ.");
    }

    private static void ExpectFailure(string json, string name, ref int checks)
    {
        try
        {
            WorkIqSafety.ParseAndValidate(json);
            throw new InvalidOperationException($"FAIL: {name}");
        }
        catch (InvalidOperationException)
        {
            checks++;
        }
    }

    private static void Check(bool condition, string name, ref int checks)
    {
        if (!condition) throw new InvalidOperationException($"FAIL: {name}");
        checks++;
    }
}
