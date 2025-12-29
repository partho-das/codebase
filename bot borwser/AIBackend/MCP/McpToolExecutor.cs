using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace AIBackend.MCP
{
    public sealed class McpToolExecutor : IToolExecutor
    {
        private readonly McpClient _client;
        public readonly IList<McpClientTool> _tools;

        public IList<McpClientTool> Tools => _tools;

        public McpToolExecutor()
        {
            var args = new string[] { "C:\\Users\\partho\\Desktop\\Code\\codebase\\C#Practice\\AIBackend\\FirstMCP\\FirstMCP\\FirstMCP.csproj" };
            var (command, arguments) = GetCommandAndArguments(args);
            var clientTransport = new StdioClientTransport(new()
            {
                Name = "First Servier",
                Command = command,
                Arguments = arguments,
            });
            _client = McpClient.CreateAsync(clientTransport).GetAwaiter().GetResult();

            _tools = _client.ListToolsAsync().GetAwaiter().GetResult();

        }

        public async Task<string> ExecuteAsync(
            string toolName,
            Dictionary<string, object?> paramemters)
        {
     
            var result = await _client.CallToolAsync(
                toolName,
                paramemters);

            return JsonSerializer.Serialize(result);
        }

        static (string command, string[] arguments) GetCommandAndArguments(string[] args)
        {
            return args switch
            {
                [var script] when script.EndsWith(".py") => ("python", args),
                [var script] when script.EndsWith(".js") => ("node", args),
                [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script, "--no-build"]),
                _ => throw new NotSupportedException("An unsupported server script was provided. Supported scripts are .py, .js, or .csproj")
            };
        }

    }
}
