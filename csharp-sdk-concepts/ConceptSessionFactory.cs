using System.ComponentModel;
using GitHub.Copilot;
using GitHub.Copilot.Rpc;
using Microsoft.Extensions.AI;

namespace CsharpSdkConcepts;

#pragma warning disable GHCP001 // Las decisiones personalizadas son necesarias para enseñar permisos acotados.

public static class ConceptSessionFactory
{
    private const string ToolName = "sdk_concept_lookup";

    public static SessionConfig Create(ConceptStage stage, string modelId)
    {
        var config = Base(stage, modelId);

        switch (stage.Id)
        {
            case "01":
                config.Streaming = true;
                break;
            case "02":
                config.SystemMessage = new SystemMessageConfig
                {
                    Mode = SystemMessageMode.Replace,
                    Content = "Eres un instructor para principiantes. Responde en exactamente dos frases cortas y en espanol."
                };
                break;
            case "03":
                config.Tools = [CreateConceptTool(skipPermission: true)];
                config.AvailableTools = [ToolName];
                break;
            case "04":
                config.Tools = [CreateConceptTool(skipPermission: false)];
                config.AvailableTools = [ToolName];
                config.OnPermissionRequest = LocalToolPermissionHandler();
                break;
            case "05":
                config.Streaming = true;
                config.McpServers = new Dictionary<string, McpServerConfig>
                {
                    ["wikipedia"] = WikipediaServer()
                };
                config.AvailableTools = ["wikipedia-search", "wikipedia-readArticle"];
                config.OnPermissionRequest = WikipediaPermissionHandler();
                break;
            case "06":
                config.EnableSkills = true;
                config.SkillDirectories = [SkillsDirectory()];
                break;
            case "07":
                ConfigureWorkIq(config);
                break;
        }

        return config;
    }

    private static SessionConfig Base(ConceptStage stage, string modelId) => new()
    {
        ClientName = $"csharp-sdk-concepts-{stage.Id}",
        Model = modelId,
        Streaming = false,
        EnableConfigDiscovery = false,
        EnableSkills = false,
        EnableHostGitOperations = false,
        OnPermissionRequest = (_, _) => Task.FromResult(
            PermissionDecision.Reject("Esta etapa no autoriza tools."))
    };

    private static AIFunction CreateConceptTool(bool skipPermission) => CopilotTool.DefineTool(
        ([Description("Concepto: session, streaming, tool, permisos o MCP.")] string concept) =>
        {
            var explanation = concept.Trim().ToLowerInvariant() switch
            {
                "session" => "Una sesion conserva configuracion, contexto y eventos de una conversacion.",
                "streaming" => "Streaming entrega deltas mientras el modelo genera la respuesta.",
                "tool" => "Una tool permite que el host ejecute codigo tipado y devuelva el resultado al modelo.",
                "permisos" or "permissions" => "El host decide cada operacion sensible antes de ejecutarla.",
                "mcp" => "MCP conecta tools externas mediante un protocolo estandar.",
                _ => "Concepto no incluido en el catalogo acotado de esta demo."
            };
            Console.WriteLine($"\n[handler:local-tool] concept={concept}");
            return Task.FromResult(explanation);
        },
        toolOptions: new CopilotToolOptions { SkipPermission = skipPermission },
        factoryOptions: new AIFunctionFactoryOptions
        {
            Name = ToolName,
            Description = "Consulta un catalogo local y de solo lectura sobre conceptos del Copilot SDK."
        });

    private static Func<PermissionRequest, PermissionInvocation, Task<PermissionDecision>>
        LocalToolPermissionHandler() => (request, _) =>
        {
            var allowed = request is PermissionRequestCustomTool { ToolName: ToolName };
            Console.WriteLine($"\n[permission:{(allowed ? "allow-once" : "deny")}] {request.Kind}");
            return Task.FromResult(allowed
                ? PermissionDecision.ApproveOnce()
                : PermissionDecision.Reject("Esta etapa autoriza solamente la tool local."));
        };

    private static McpStdioServerConfig WikipediaServer()
    {
        var server = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "museum", "node_modules", "wikipedia-mcp", "dist", "index.js"));
        if (!File.Exists(server))
            throw new FileNotFoundException("Falta Wikipedia MCP. Ejecuta npm ci en demos\\museum.", server);

        return new McpStdioServerConfig
        {
            Command = "node",
            Args = [server],
            WorkingDirectory = Directory.GetCurrentDirectory(),
            Tools = ["search", "readArticle"]
        };
    }

    private static Func<PermissionRequest, PermissionInvocation, Task<PermissionDecision>>
        WikipediaPermissionHandler() => (request, _) =>
        {
            var allowed = request is PermissionRequestMcp { ServerName: "wikipedia" } mcp &&
                mcp.ToolName is "search" or "readArticle" or "wikipedia-search" or "wikipedia-readArticle";
            Console.WriteLine($"\n[permission:{(allowed ? "allow-once" : "deny")}] {request.Kind}");
            return Task.FromResult(allowed
                ? PermissionDecision.ApproveOnce()
                : PermissionDecision.Reject("Solo se permiten busqueda y lectura en Wikipedia."));
        };

    private static string SkillsDirectory() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "Skills"));

    private static void ConfigureWorkIq(SessionConfig config)
    {
        var pluginDirectory = WorkIqSafety.GetPluginDirectory();
        config.PluginDirectories = [pluginDirectory];
        config.EnableSkills = true;
        config.AvailableTools = ["workiq-ask"];
        config.Streaming = false;
        config.OnPermissionRequest = (request, _) =>
        {
            var allowed = request is PermissionRequestMcp { ServerName: "workiq" } mcp &&
                mcp.ToolName is "ask" or "workiq-ask";
            Console.WriteLine($"\n[permission:{(allowed ? "allow-once" : "deny")}] {request.Kind}");
            return Task.FromResult(allowed
                ? PermissionDecision.ApproveOnce()
                : PermissionDecision.Reject("La demo WorkIQ permite solamente la operacion semantica ask."));
        };
        config.SystemMessage = new SystemMessageConfig
        {
            Mode = SystemMessageMode.Replace,
            Content = WorkIqSafety.SystemMessage
        };
    }
}
