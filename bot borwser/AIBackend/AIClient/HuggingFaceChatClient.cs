using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIBackend.Ai.Tools;
using AIBackend.Interfaces;
using AIBackend.Models;
using AIBackend.Services;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AIBackend.AIClient;

public class HuggingFaceChatClient : IAiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ToolRegistry _toolRegistry;


    public HuggingFaceChatClient(HttpClient http, IConfiguration cfg, ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
        _http = http;
        _apiKey = cfg["AI:HuggingFace:ApiKey"] ?? Environment.GetEnvironmentVariable("HF_API_KEY");
        _model = cfg["AI:HuggingFace:Model"] ?? "tiiuae/falcon-7b-instruct";
        if (string.IsNullOrEmpty(_apiKey))
            throw new ArgumentException("HuggingFace API key not configured. Set AI:HuggingFace:ApiKey or HF_API_KEY.");
    }

    public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
    {
        var prompt = PromptHelpers.BasicBuildPrompt(request);
        List<Message> messages = new() { new Message() { Role = "system", Content = prompt } };
        List<ToolResponse> newTollResponse = new();

        messages.Add(new Message(){Role = "user", Content = request.Message});
        bool firstMessage = true;

        while (true)
        {

            if (!firstMessage && newTollResponse.Count == 0) yield break;
            firstMessage = false;
            newTollResponse.Clear();

            await foreach (var response in GetResponseForMessages(messages, newTollResponse))
            {
                yield return response;
            }
        }
    }

    private async IAsyncEnumerable<AiResponse?> GetResponseForMessages(List<Message> messages, List<ToolResponse> toolResponses)
    {
        var body = new
        {
            model = _model,
            messages = messages,
            functions = _toolRegistry.AllTools.Select(t => new
            {
                name = t.Name,
                description = t.Description,
                parameters = t.Schema
            }).ToList(),
            tool_choice = "auto",
            max_tokens = 1000,
            temperature = 0.3,
            stream = true
        };


        var jsonBody = JsonConvert.SerializeObject(body);
        var req = new HttpRequestMessage(HttpMethod.Post, $"https://router.huggingface.co/v1/chat/completions")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        req.Headers.Add("Authorization", $"Bearer {_apiKey}");

        using var response = await _http.SendAsync(req);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string accumulatedText = "";


        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (!line.StartsWith("data: ")) // no event streaming
            {
                accumulatedText += line;
                continue;
            }

            var jsonPart = line["data: ".Length..];
            if (jsonPart == "[DONE]")
                break;
            using var doc = JsonDocument.Parse(jsonPart);
            var root = doc.RootElement;

            if (!TryGetDelta(root, out var delta))
                continue;

            foreach (var chunkResponse in ExtractTextResponses(delta))
                yield return chunkResponse;

            if (delta.TryGetProperty("tool_calls", out var toolCallsElem) &&
                toolCallsElem.ValueKind == JsonValueKind.Array)
            {
                await foreach (var toolResponse in HandleToolCallsAsync(toolCallsElem))
                {
                    messages.Add(new Message()
                    {
                        Role = "system",
                        Content = $"Tool Response:  ToolName = {toolResponse.ToolName}, Response = {JsonSerializer.Serialize(toolResponse.Result) }",
                    });
                    toolResponses.Add(toolResponse);
                }
                    
            }


        }

        yield break;

    }

    private static bool TryGetDelta(JsonElement root, out JsonElement delta)
    {
        delta = default;

        if (!root.TryGetProperty("choices", out var choices) ||
            choices.GetArrayLength() == 0)
            return false;

        return choices[0].TryGetProperty("delta", out delta);
    }

    private IEnumerable<AiResponse?> ExtractTextResponses(JsonElement delta)
    {
        yield return CreateResponseIfExists(delta, "content", AiResponse.ResponseType.NormalResponse);
        yield return CreateResponseIfExists(delta, "reasoning", AiResponse.ResponseType.Reasoning);
        yield return CreateResponseIfExists(delta, "thinking", AiResponse.ResponseType.Thinking);
    }

    private static AiResponse? CreateResponseIfExists(
        JsonElement delta,
        string propertyName,
        AiResponse.ResponseType type)
    {
        if (!delta.TryGetProperty(propertyName, out var elem))
            return default;
        
        var text = elem.GetString();

        Console.WriteLine($"Response:<^>{elem}<^>");
        if (string.IsNullOrWhiteSpace(text))
            return default;
        return new AiResponse
        {
            ReplyText = text,
            Type = type,
            Actions = new (),
        };

    }

    private async IAsyncEnumerable<ToolResponse> HandleToolCallsAsync(JsonElement toolCallsElem)
    {
        var toolCalls = JsonNode.Parse(toolCallsElem.GetRawText())!.AsArray();

        foreach (var toolCall in toolCalls)
        {
            var toolName = toolCall?["function"]?["name"]?.GetValue<string>();
            var argumentsJson = toolCall?["function"]?["arguments"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(toolName))
                continue;

            var tool = _toolRegistry.GetTool(toolName);
           
            if (tool == null)
            {
                Console.WriteLine($"Tool {toolName} not found in registry.");
                continue;
            }
            var result = await tool.ExecuteAsync(argumentsJson);
            yield return new ToolResponse()
            {
                Result = JsonSerializer.Serialize(result),
                ToolName = toolName
            };
        }
    }


}

public class ToolResponse
{
    public object Result { get; set; }
    public object ToolName { get; set; }
}

public class Message
{
    [JsonProperty("role")]
    public string? Role { get; set; }
    [JsonProperty("content")]
    public string? Content { get; set; }
    //[JsonProperty("name")]
    //public string? Name { get; set; }
}