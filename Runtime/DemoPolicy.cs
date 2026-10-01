using System.Text.Json;
using GitHub.Copilot;
using GitHub.Copilot.Rpc;

namespace AccessibilityDemo.Runtime;

#pragma warning disable GHCP001 // Scoped decisions use the SDK's experimental permission contract.
public sealed class DemoPolicy(bool rules, bool browser)
{
    public static readonly Uri Target = new("http://127.0.0.1:4173/");
    public int ApprovedNavigations { get; private set; }
    public int RejectedNavigations { get; private set; }

    public Task<PermissionDecision> Decide(PermissionRequest request, PermissionInvocation invocation)
    {
        var allowed = Allows(request);
        if (request is PermissionRequestMcp)
        {
            if (allowed) ApprovedNavigations++;
            else if (request is PermissionRequestMcp denied &&
                     denied.ServerName == "playwright" &&
                     denied.ToolName is "browser_navigate" or "playwright-browser_navigate" &&
                     HasUrl(denied.Args, "http://127.0.0.1:4173/blocked"))
                RejectedNavigations++;
        }

        Console.WriteLine($"\n[permission:{(allowed ? "allow-once" : "deny")}] {request.Kind}");
        return Task.FromResult(allowed
            ? PermissionDecision.ApproveOnce()
            : PermissionDecision.Reject("Demo policy: only the exact local target and scoped read-only tools are authorized."));
    }

    public bool Allows(PermissionRequest request) => request switch
    {
        PermissionRequestMcp { ServerName: "playwright" } mcp when browser =>
            (mcp.ToolName is "browser_navigate" or "playwright-browser_navigate") &&
            IsExactTarget(mcp.Args),
        PermissionRequestCustomTool { ToolName: "accessibility_rule_lookup" } tool when rules =>
            IsKnownCriterion(tool.Args),
        PermissionRequestCustomTool { ToolName: "read_latest_accessibility_snapshot" } when browser => true,
        _ => false
    };

    private static bool IsExactTarget(JsonElement? args) =>
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String &&
        Uri.TryCreate(url.GetString(), UriKind.Absolute, out var requested) &&
        requested.AbsoluteUri.Equals(Target.AbsoluteUri, StringComparison.Ordinal);

    private static bool HasUrl(JsonElement? args, string expected) =>
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String &&
        url.GetString() == expected;

    private static bool IsKnownCriterion(JsonElement? args) =>
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("criterion", out var criterion) &&
        criterion.ValueKind == JsonValueKind.String &&
        criterion.GetString() is "1.1.1" or "1.3.1" or "1.4.3" or "2.4.7" or "3.3.2" or "4.1.2";
}
#pragma warning restore GHCP001
