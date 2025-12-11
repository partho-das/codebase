using Swashbuckle.Swagger.Model;

namespace AIBackend.Interfaces
{
    public interface IAgentTool
    {
        string Name { get; }
        string Description { get; }
        public object Schema { get; }
        Task<object?> ExecuteAsync(string? input);

    }
}
