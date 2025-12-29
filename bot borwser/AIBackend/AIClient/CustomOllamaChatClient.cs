using AIBackend.Ai.Tools;
using AIBackend.MCP;
using AIBackend.Models;
using AIBackend.Models.Ollama;
using AIBackend.Services;
using ModelContextProtocol.Protocol;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIBackend.AIClient
{
    public class CustomOllamaChatClient : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _baseUrl;
        private readonly ToolRegistry _toolRegistry;
        private readonly IToolExecutor _mcpToolExecutor;

        public CustomOllamaChatClient(HttpClient httpClient, IConfiguration cfg, ToolRegistry toolRegistry, IToolExecutor mcpToolExecutor)
        {
            _httpClient = httpClient;
            _toolRegistry = toolRegistry;
            _model = cfg["AI:Ollama:Model"] ?? "llama3.2:3b";
            _baseUrl = cfg["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
            _mcpToolExecutor = mcpToolExecutor;
        }

        public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
        {
            var systemPrompt = PromptHelpers.BasicBuildPrompt(request);
            
            // Build messages
            var messages = new List<OllamaMessage>
            {
                new OllamaMessage
                {
                    Role = "system",
                    Content = systemPrompt
                },
                new OllamaMessage
                {
                    Role = "user",
                    Content = request.Message
                }
            };

            // Track token usage across all iterations
            var totalStats = new TokenUsageStats();

            bool continueLoop = true;
            int maxIterations = 10;
            int iteration = 0;

            while (continueLoop && iteration < maxIterations)
            {
                iteration++;

                // Build Ollama request
                var ollamaRequest = new OllamaChatRequest
                {
                    Model = _model,
                    Messages = messages,
                    Stream = true,
                    Think = true, // Enable thinking/reasoning
                    Options = new OllamaOptions
                    {
                        Temperature = 0.2f,
                        NumPredict = 10000
                    },
                    Tools = BuildTools()
                };

                // Make request and stream response
                var hasToolCalls = false;

                await foreach (var response in StreamChatAsync(ollamaRequest))
                {
                    if (response == null)
                        continue;

                    // Accumulate token statistics
                    if (response.Stats != null)
                    {
                        totalStats.PromptEvalCount += response.Stats.PromptEvalCount ?? 0;
                        totalStats.EvalCount += response.Stats.EvalCount ?? 0;
                        totalStats.TotalDuration += response.Stats.TotalDuration ?? 0;
                        totalStats.LoadDuration += response.Stats.LoadDuration ?? 0;
                        totalStats.PromptEvalDuration += response.Stats.PromptEvalDuration ?? 0;
                        totalStats.EvalDuration += response.Stats.EvalDuration ?? 0;
                    }
                        
                    // Handle thinking content
                    if (!string.IsNullOrEmpty(response.Thinking))
                    {
                        yield return new AiResponse
                        {
                            ReplyText = response.Thinking,
                            Type = AiResponse.ResponseType.Thinking,
                            Actions = new()
                        };
                    }

                    // Handle normal content
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        yield return new AiResponse
                        {
                            ReplyText = response.Content,
                            Type = AiResponse.ResponseType.NormalResponse,
                            Actions = new()
                        };
                    }

                    // Handle tool calls
                    if (response.ToolCalls != null && response.ToolCalls.Count > 0)
                    {
                        hasToolCalls = true;

                        // Add assistant message with tool calls to history
                        messages.Add(new OllamaMessage
                        {
                            Role = "assistant",
                            Content = response.Content ?? string.Empty,
                            ToolCalls = response.ToolCalls
                        });

                        // Execute tools
                        foreach (var toolCall in response.ToolCalls)
                        {
                            yield return new AiResponse
                            {
                                ReplyText = $"🔧 Calling tool: {toolCall.Function.Name}\n",
                                Type = AiResponse.ResponseType.ToolResponse,
                                Actions = new()
                            };

                            // Execute the tool
                            string toolResult;

                            if (_mcpToolExecutor.Tools.Select(t => t.Name).Contains(toolCall.Function.Name))
                            {
                                var jsonText = JsonSerializer.Serialize(toolCall.Function.Arguments);
                                var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonText)!;
                                toolResult = await _mcpToolExecutor.ExecuteAsync(toolCall.Function.Name, arguments);
                            }
                            else
                            {
                                toolResult = await ExecuteToolAsync(toolCall);
                            }

                            // Add tool result to messages
                            messages.Add(new OllamaMessage
                            {
                                Role = "tool",
                                Content = toolResult
                            });

                            yield return new AiResponse
                            {
                                ReplyText = $"✅ Tool '{toolCall.Function.Name}' executed successfully\n\n",
                                Type = AiResponse.ResponseType.ToolResponse,
                                Actions = new()
                            };
                        }
                    }

                    // If response is done and no tool calls, exit loop
                    if (response.Done && !hasToolCalls)
                    {
                        continueLoop = false;
                    }
                }

                // If no tool calls were made, exit
                if (!hasToolCalls)
                {
                    continueLoop = false;
                }
            }

            if (iteration >= maxIterations)
            {
                yield return new AiResponse
                {
                    ReplyText = "⚠️ Maximum iterations reached. Please try rephrasing your question.",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };
            }

            // Send token usage statistics at the end in formatted markdown
            if (totalStats.HasData)
            {
                // Add spacing
                yield return new AiResponse
                {
                    ReplyText = "\n\n---\n\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                // Header with emoji
                yield return new AiResponse
                {
                    ReplyText = "## 📊 Token Usage Statistics\n\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                // Token usage table
                yield return new AiResponse
                {
                    ReplyText = "### Token Consumption\n\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = "| Metric | Count |\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = "|--------|-------|\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = $"| 📥 **Input Tokens** (Prompt) | `{totalStats.PromptEvalCount:N0}` |\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = $"| 📤 **Output Tokens** (Completion) | `{totalStats.EvalCount:N0}` |\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = $"| 🔢 **Total Tokens** | `{totalStats.TotalTokens:N0}` |\n\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                // Performance metrics
                if (totalStats.TotalDuration > 0)
                {
                    var totalSeconds = totalStats.TotalDuration / 1_000_000_000.0;
                    var tokensPerSecond = totalStats.EvalCount / (totalStats.EvalDuration / 1_000_000_000.0);
                    var loadSeconds = totalStats.LoadDuration / 1_000_000_000.0;
                    var promptEvalSeconds = totalStats.PromptEvalDuration / 1_000_000_000.0;
                    var evalSeconds = totalStats.EvalDuration / 1_000_000_000.0;

                    yield return new AiResponse
                    {
                        ReplyText = "### ⚡ Performance Metrics\n\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    yield return new AiResponse
                    {
                        ReplyText = "| Metric | Value |\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    yield return new AiResponse
                    {
                        ReplyText = "|--------|-------|\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    yield return new AiResponse
                    {
                        ReplyText = $"| ⏱️ **Total Duration** | `{totalSeconds:F2}s` |\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    if (loadSeconds > 0.001)
                    {
                        yield return new AiResponse
                        {
                            ReplyText = $"| 🔄 **Model Load Time** | `{loadSeconds:F3}s` |\n",
                            Type = AiResponse.ResponseType.NormalResponse,
                            Actions = new()
                        };
                    }

                    yield return new AiResponse
                    {
                        ReplyText = $"| 📖 **Prompt Processing** | `{promptEvalSeconds:F3}s` |\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    yield return new AiResponse
                    {
                        ReplyText = $"| ✍️ **Token Generation** | `{evalSeconds:F3}s` |\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    yield return new AiResponse
                    {
                        ReplyText = $"| 🚀 **Generation Speed** | `{tokensPerSecond:F1}` tokens/s |\n\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };
                }

                // Model info
                yield return new AiResponse
                {
                    ReplyText = "### 🤖 Model Information\n\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = $"- **Model**: `{_model}`\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                yield return new AiResponse
                {
                    ReplyText = $"- **Iterations**: `{iteration}` / `{maxIterations}`\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };

                // Calculate efficiency
                if (totalStats.TotalTokens > 0 && totalStats.TotalDuration > 0)
                {
                    var totalSeconds = totalStats.TotalDuration / 1_000_000_000.0;
                    var tokensPerSecondOverall = totalStats.TotalTokens / totalSeconds;

                    yield return new AiResponse
                    {
                        ReplyText = $"- **Overall Throughput**: `{tokensPerSecondOverall:F1}` tokens/s\n",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };
                }

                // Footer
                yield return new AiResponse
                {
                    ReplyText = "\n---\n",
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };
            }
        }

        private async IAsyncEnumerable<OllamaStreamResponse?> StreamChatAsync(OllamaChatRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat")
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                OllamaStreamResponse? streamResponse = null;
                try
                {
                    var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(line);
                    if (chatResponse != null)
                    {
                        streamResponse = new OllamaStreamResponse
                        {
                            Content = chatResponse.Message.Content,
                            Thinking = chatResponse.Message.Thinking,
                            ToolCalls = chatResponse.Message.ToolCalls,
                            Done = chatResponse.Done,
                            Stats = chatResponse.Done ? new ResponseStats
                            {
                                PromptEvalCount = chatResponse.PromptEvalCount,
                                EvalCount = chatResponse.EvalCount,
                                TotalDuration = chatResponse.TotalDuration,
                                LoadDuration = chatResponse.LoadDuration,
                                PromptEvalDuration = chatResponse.PromptEvalDuration,
                                EvalDuration = chatResponse.EvalDuration
                            } : null
                        };
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parse error: {ex.Message}");
                    continue;
                }

                yield return streamResponse;
            }
        }

        private List<OllamaTool> BuildTools()
        {
            var tools = new List<OllamaTool>();

            foreach (var tool in _toolRegistry.AllTools)
            {
                tools.Add(new OllamaTool
                {
                    Type = "function",
                    Function = new OllamaToolFunction
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = tool.Schema
                    }
                });
            }

            var mcpTools = _mcpToolExecutor.Tools.Select(tool =>
            new OllamaTool()
            {
                Type = "function",
                Function = new OllamaToolFunction
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Parameters = JsonSerializer.SerializeToElement(tool.JsonSchema)
                }
            }
           ).ToList();
            tools.AddRange(mcpTools);
            return tools;
        }

        private async Task<string> ExecuteToolAsync(OllamaToolCall toolCall)
        {
            var tool = _toolRegistry.GetTool(toolCall.Function.Name);
            if (tool == null)
            {
                return JsonSerializer.Serialize(new { error = $"Tool '{toolCall.Function.Name}' not found" });
            }

            try
            {
                // Convert arguments to JSON string
                var argumentsJson = JsonSerializer.Serialize(toolCall.Function.Arguments);
                
                // Execute the tool
                var result = await tool.ExecuteAsync(argumentsJson);
                
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }

    // Helper class for streaming responses
    public class OllamaStreamResponse
    {
        public string? Content { get; set; }
        public string? Thinking { get; set; }
        public List<OllamaToolCall>? ToolCalls { get; set; }
        public bool Done { get; set; }
        public ResponseStats? Stats { get; set; }
    }

    // Helper class for response statistics
    public class ResponseStats
    {
        public int? PromptEvalCount { get; set; }
        public int? EvalCount { get; set; }
        public long? TotalDuration { get; set; }
        public long? LoadDuration { get; set; }
        public long? PromptEvalDuration { get; set; }
        public long? EvalDuration { get; set; }
    }

    // Helper class to accumulate token usage across iterations
    public class TokenUsageStats
    {
        public int PromptEvalCount { get; set; }
        public int EvalCount { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public long PromptEvalDuration { get; set; }
        public long EvalDuration { get; set; }

        public int TotalTokens => PromptEvalCount + EvalCount;
        public bool HasData => PromptEvalCount > 0 || EvalCount > 0;
    }
}
