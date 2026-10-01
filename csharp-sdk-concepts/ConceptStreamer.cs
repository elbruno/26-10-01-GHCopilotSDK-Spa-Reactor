using System.Text;
using GitHub.Copilot;

namespace CsharpSdkConcepts;

public static class ConceptStreamer
{
    public static async Task<string> RunAsync(
        CopilotSession session,
        string prompt,
        bool printContent,
        CancellationToken cancellationToken)
    {
        var response = new StringBuilder();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var receivedDelta = false;
        string? failedTool = null;

        using var subscription = session.On<SessionEvent>(sessionEvent =>
        {
            switch (sessionEvent)
            {
                case AssistantMessageDeltaEvent delta when !string.IsNullOrEmpty(delta.Data.DeltaContent):
                    receivedDelta = true;
                    response.Append(delta.Data.DeltaContent);
                    if (printContent) Console.Write(delta.Data.DeltaContent);
                    break;
                case AssistantMessageEvent message when !receivedDelta && !string.IsNullOrEmpty(message.Data.Content):
                    response.Clear();
                    response.Append(message.Data.Content);
                    if (printContent) Console.Write(message.Data.Content);
                    break;
                case ToolExecutionStartEvent tool:
                    Console.WriteLine($"\n[tool:start] {tool.Data.ToolName}");
                    break;
                case ToolExecutionCompleteEvent tool:
                    Console.WriteLine($"[tool:done] success={tool.Data.Success}");
                    if (!tool.Data.Success)
                        failedTool = tool.Data.ToolCallId;
                    break;
                case SessionIdleEvent:
                    completed.TrySetResult();
                    break;
                case SessionErrorEvent error:
                    completed.TrySetException(new InvalidOperationException(error.Data.Message));
                    break;
            }
        });

        await session.SendAsync(new MessageOptions { Prompt = prompt }, cancellationToken);
        var finished = await Task.WhenAny(
            completed.Task,
            Task.Delay(TimeSpan.FromMinutes(3), cancellationToken));
        if (finished != completed.Task)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("La respuesta alcanzo el limite de tiempo.");
        }

        await completed.Task;
        if (failedTool is not null)
            throw new InvalidOperationException(
                "Una tool requerida fallo; la aplicacion bloqueo cualquier respuesta posterior.");
        if (printContent) Console.WriteLine();
        return response.ToString();
    }
}
