// Politica determinista de Accessibility.
// Enseña que OnPermissionRequest no es decorativo: el host decide cada tool
// permitida, cuenta aprobaciones y rechazos, y bloquea navegaciones fuera del
// objetivo local usado en la demo.
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
        // El modelo puede pedir una accion; PermissionDecision es la respuesta del host.
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
            // ApproveOnce autoriza esta invocacion, no concede permiso permanente.
            ? PermissionDecision.ApproveOnce()
            : PermissionDecision.Reject("Demo policy: only the exact local target and scoped read-only tools are authorized."));
    }

    // La allowlist separa capacidades por etapa para enseñar el crecimiento progresivo.
    public bool Allows(PermissionRequest request) => request switch
    {
        // MCP solo puede navegar a la URL exacta controlada por la demo.
        PermissionRequestMcp { ServerName: "playwright" } mcp when browser =>
            (mcp.ToolName is "browser_navigate" or "playwright-browser_navigate") &&
            IsExactTarget(mcp.Args),
        // La tool local acepta solo criterios WCAG conocidos por el catalogo.
        PermissionRequestCustomTool { ToolName: "accessibility_rule_lookup" } tool when rules =>
            IsKnownCriterion(tool.Args),
        PermissionRequestCustomTool { ToolName: "read_latest_accessibility_snapshot" } when browser => true,
        _ => false
    };

    private static bool IsExactTarget(JsonElement? args) =>
        // Se compara la URL normalizada completa para impedir querystrings o rutas extra.
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String &&
        Uri.TryCreate(url.GetString(), UriKind.Absolute, out var requested) &&
        requested.AbsoluteUri.Equals(Target.AbsoluteUri, StringComparison.Ordinal);

    private static bool HasUrl(JsonElement? args, string expected) =>
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String &&
        url.GetString() == expected;

    private static bool IsKnownCriterion(JsonElement? args) =>
        // El host valida argumentos porque no confia en parametros generados por el modelo.
        args is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("criterion", out var criterion) &&
        criterion.ValueKind == JsonValueKind.String &&
        criterion.GetString() is "1.1.1" or "1.3.1" or "1.4.3" or "2.4.7" or "3.3.2" or "4.1.2";
}
#pragma warning restore GHCP001
