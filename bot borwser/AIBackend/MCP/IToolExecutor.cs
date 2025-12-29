using ModelContextProtocol.Client;

namespace AIBackend.MCP
{
    public interface IToolExecutor
    {
       IList<McpClientTool> Tools { get; }
        Task<string> ExecuteAsync(
            string toolName,
             Dictionary<string, object?> parameters);
    }

}