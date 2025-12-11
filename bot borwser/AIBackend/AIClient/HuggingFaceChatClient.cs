using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIBackend.Ai.Tools;
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

    public async IAsyncEnumerable<AiResponse> AnalyzeAsync(AiRequest request)
    {
        var prompt = PromptHelpers.BasicBuildPrompt(request);

        var body = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
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
       // response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string accumulatedText = "";

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.Contains("tool"))
            {
                var x = 5;
            }
            if (line.StartsWith("data: "))
            {
                var jsonPart = line.Substring("data: ".Length);
                if (jsonPart == "[DONE]") break;

                using var doc = JsonDocument.Parse(jsonPart);
                var root = doc.RootElement;

                if (!root.TryGetProperty("choices", out var choicesElem) || choicesElem.GetArrayLength() == 0)
                    continue;

                var delta = choicesElem[0].GetProperty("delta");

                // Append AI text if available
                if (delta.TryGetProperty("content", out var contentElement))
                {
                    yield return new AiResponse
                    {
                        ReplyText = contentElement.GetString(),
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new List<ActionCommand>()
                    };
                }

                if (delta.TryGetProperty("reasoning", out var reasoning))
                {
                    yield return new AiResponse
                    {
                        ReplyText = reasoning.GetString(),
                        Type = AiResponse.ResponseType.Reasoning,
                        Actions = new List<ActionCommand>()
                    };
                }

                if (delta.TryGetProperty("thinking", out var thinking))
                {
                    yield return new AiResponse
                    {
                        ReplyText = thinking.GetString(),
                        Type = AiResponse.ResponseType.Thinking,
                        Actions = new List<ActionCommand>()
                    };
                }
                if (delta.TryGetProperty("tool_calls", out var toolCallsElem) && toolCallsElem.ValueKind == JsonValueKind.Array)
                {
                    var y = JsonNode.Parse(toolCallsElem.GetRawText()).AsArray();
                    foreach (var toolCall in y)
                    {
                        var toolName = toolCall?["function"]?["name"]?.GetValue<string>();
                        var argumentsJson = toolCall?["function"]?["arguments"]?.GetValue<string>();

                        var tool = _toolRegistry.GetTool(toolName);
                        if (tool != null)
                        {
                            // Deserialize to object dynamically 
                            var result = await tool.ExecuteAsync(argumentsJson);
                            yield return new AiResponse
                            {
                                ReplyText = JsonSerializer.Serialize(result),
                                Type = AiResponse.ResponseType.NormalResponse,
                                Actions = new List<ActionCommand>()
                            };
                        }
                        else
                        {
                            Console.WriteLine($"Tool {toolName} not found in registry.");
                        }
                    }
                }

            }
            else
            {
                // Fallback: append raw line
                accumulatedText += line;
            }
        }

        yield return new AiResponse
        {
            ReplyText = accumulatedText,
            Actions = new List<ActionCommand>()
        };
    }
}