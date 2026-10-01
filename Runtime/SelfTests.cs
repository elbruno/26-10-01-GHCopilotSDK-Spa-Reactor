using System.Text.Json;
using GitHub.Copilot;

namespace AccessibilityDemo.Runtime;

public static class SelfTests
{
    public static void Run()
    {
        var count = 0;
        void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException($"FAIL: {description}");
            count++;
        }
        var policy = new DemoPolicy(true, true);
        PermissionRequestMcp Navigation(string url) => new()
        {
            ServerName = "playwright", ToolName = "browser_navigate",
            ToolTitle = "Navigate", ReadOnly = false,
            Args = JsonSerializer.SerializeToElement(new { url })
        };
        Check(policy.Allows(Navigation("http://127.0.0.1:4173/")), "exact target");
        foreach (var url in new[] { "https://example.com/", "http://127.0.0.1:4173/blocked",
                     "http://127.0.0.1:4174/", "http://localhost:4173/",
                     "http://127.0.0.1:4173/?q=1", "http://127.0.0.1:4173/#x",
                     "http://user@127.0.0.1:4173/", "file:///C:/private" })
            Check(!policy.Allows(Navigation(url)), "deny non-exact target");
        var wrongTool = Navigation(DemoPolicy.Target.AbsoluteUri);
        wrongTool.ToolName = "browser_evaluate";
        Check(!policy.Allows(wrongTool), "deny browser evaluate");
        var wrongServer = Navigation(DemoPolicy.Target.AbsoluteUri);
        wrongServer.ServerName = "another-server";
        Check(!policy.Allows(wrongServer), "deny different server");
        var missingUrl = Navigation(DemoPolicy.Target.AbsoluteUri);
        missingUrl.Args = JsonSerializer.SerializeToElement(new { });
        Check(!policy.Allows(missingUrl), "deny missing URL");
        Check(!policy.Allows(new PermissionRequestCustomTool
        {
            ToolName = "unknown", ToolDescription = "Unknown tool"
        }), "deny unknown custom tool");
        Check(!new DemoPolicy(false, false).Allows(Navigation(DemoPolicy.Target.AbsoluteUri)),
            "no browser in early stages");
        Check(policy.Allows(new PermissionRequestCustomTool
        {
            ToolName = "accessibility_rule_lookup", ToolDescription = "Read-only rules",
            Args = JsonSerializer.SerializeToElement(new { criterion = "4.1.2" })
        }), "allow known rule");
        Check(!policy.Allows(new PermissionRequestCustomTool
        {
            ToolName = "accessibility_rule_lookup", ToolDescription = "Read-only rules",
            Args = JsonSerializer.SerializeToElement(new { criterion = "../../secret" })
        }), "deny invalid rule");
        Check(!new DemoPolicy(false, false).Allows(new PermissionRequestCustomTool
        {
            ToolName = "read_latest_accessibility_snapshot", ToolDescription = "Snapshot"
        }), "deny snapshot in early stages");
        Check(RuleCatalog.Lookup("4.1.2").Criterion == "4.1.2", "rule lookup");
        try { RuleCatalog.Lookup("unknown"); throw new InvalidOperationException("Invalid rule accepted."); }
        catch (ArgumentException) { count++; }

        var directory = Path.Combine(Path.GetTempPath(), $"copilot-demo-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var reader = new SnapshotReader(directory);
            try { reader.Read(); throw new InvalidOperationException("Missing snapshot accepted."); }
            catch (FileNotFoundException) { count++; }
            var file = Path.Combine(directory, "page-test.yml");
            File.WriteAllText(file, "- textbox");
            Check(reader.Read() == "- textbox", "read new snapshot");
            try { reader.Read(); throw new InvalidOperationException("Stale snapshot accepted."); }
            catch (FileNotFoundException) { count++; }
            File.WriteAllText(file, new string('x', 1_000_001));
            try { new SnapshotReader(directory).Read(); throw new InvalidOperationException("Oversize accepted."); }
            catch (FileNotFoundException) { count++; }
        }
        finally { Directory.Delete(directory, recursive: true); }
        Console.WriteLine($"[self-test] PASS {count} checks; sin llamadas al modelo.");
    }
}
