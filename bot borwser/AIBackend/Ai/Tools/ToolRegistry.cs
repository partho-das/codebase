using AIBackend.Interfaces;
using System.Collections.Generic;

namespace AIBackend.Ai.Tools
{
    public class ToolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object Schema { get; set; }
    }

    public class ToolRegistry
    {
        private readonly Dictionary<string, IAgentTool> _tools = new();

        // Register a tool
        public void Register(IAgentTool tool)
        {
            _tools[tool.Name] = tool;
        }

        // Retrieve a tool by name
        public IAgentTool? GetTool(string name)
        {
            if (name == null) return default;
            _tools.TryGetValue(name, out var tool);
            return tool;
        }

        // Return all registered tools
        public IEnumerable<IAgentTool> AllTools => _tools.Values;

        // Return all tools as structured info: name, description, schema
        public IEnumerable<ToolInfo> GetToolInfos()
        {
            foreach (var tool in _tools.Values)
            {
                yield return new ToolInfo
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Schema = tool.Schema
                };
            }
        }
    }
}