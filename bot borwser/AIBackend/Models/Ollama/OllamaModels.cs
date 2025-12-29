using System.Text.Json.Serialization;

namespace AIBackend.Models.Ollama
{
    // Request Models
    public class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OllamaMessage> Messages { get; set; } = new();

        [JsonPropertyName("tools")]
        public List<OllamaTool>? Tools { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = true;

        [JsonPropertyName("think")]
        public object? Think { get; set; } // Can be bool or string ("high", "medium", "low")

        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }
    }

    public class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // "system", "user", "assistant", "tool"

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    public class OllamaToolCall
    {
        [JsonPropertyName("function")]
        public OllamaFunction Function { get; set; } = new();
    }

    public class OllamaFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("arguments")]
        public object? Arguments { get; set; }
    }

    public class OllamaTool
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public OllamaToolFunction Function { get; set; } = new();
    }

    public class OllamaToolFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public object Parameters { get; set; } = new { };
    }

    public class OllamaOptions
    {
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        [JsonPropertyName("num_predict")]
        public int? NumPredict { get; set; }

        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }

        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }
    }

    // Response Models
    public class OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public OllamaResponseMessage Message { get; set; } = new();

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        [JsonPropertyName("total_duration")]
        public long? TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long? LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; set; }

        [JsonPropertyName("prompt_eval_duration")]
        public long? PromptEvalDuration { get; set; }

        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; set; }

        [JsonPropertyName("eval_duration")]
        public long? EvalDuration { get; set; }
    }

    public class OllamaResponseMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }
    }
}
