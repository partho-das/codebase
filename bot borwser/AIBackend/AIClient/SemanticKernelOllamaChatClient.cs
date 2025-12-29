using AIBackend.Ai.Tools;
using AIBackend.Models;
using AIBackend.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text.Json;

namespace AIBackend.AIClient
{
    public class SemanticKernelOllamaChatClient : IAiService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ToolRegistry _toolRegistry;

        public SemanticKernelOllamaChatClient(IConfiguration cfg, ToolRegistry toolRegistry)
        {
            _toolRegistry = toolRegistry;
            
            var model = cfg["AI:Ollama:Model"] ?? "llama3.2:3b";
            var baseUrl = cfg["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
            
            // Remove '/api' suffix if present for Semantic Kernel
            if (baseUrl.EndsWith("/api"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 4);
            }

            #pragma warning disable SKEXP0070
            // Create kernel with Ollama chat completion
            var builder = Kernel.CreateBuilder();
            builder.AddOllamaChatCompletion(
                modelId: model,
                endpoint: new Uri(baseUrl)
            );

            // Register tools as Kernel functions
            foreach (var tool in _toolRegistry.AllTools)
            {
                RegisterToolAsKernelFunction(builder, tool);
            }

            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            #pragma warning restore SKEXP0070
        }

        private void RegisterToolAsKernelFunction(IKernelBuilder builder, Interfaces.IAgentTool tool)
        {
            // Create wrapper method that calls the tool's ExecuteAsync
            var toolCopy = tool; // Capture for closure
            
            var function = KernelFunctionFactory.CreateFromMethod(
                method: async (string input) => 
                {
                    try
                    {
                        var result = await toolCopy.ExecuteAsync(input);
                        return JsonSerializer.Serialize(result);
                    }
                    catch (Exception ex)
                    {
                        return JsonSerializer.Serialize(new { error = ex.Message });
                    }
                },
                functionName: tool.Name,
                description: tool.Description,
                parameters: new[]
                {
                    new KernelParameterMetadata("input")
                    {
                        Description = "JSON string containing the function parameters",
                        IsRequired = true
                    }
                },
                returnParameter: new KernelReturnParameterMetadata
                {
                    Description = "JSON string containing the function result"
                }
            );

            builder.Plugins.AddFromFunctions(tool.Name + "_Plugin", new[] { function });
        }

        public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
        {
            var systemPrompt = PromptHelpers.BasicBuildPrompt(request);
            
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(request.Message);

            #pragma warning disable SKEXP0070
            // Configure execution settings with manual function calling for better control
            var executionSettings = new OllamaPromptExecutionSettings
            {
                Temperature = 0.2f,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false)
            };
            #pragma warning restore SKEXP0070

            bool continueLoop = true;
            int maxIterations = 10; // Prevent infinite loops
            int iteration = 0;

            while (continueLoop && iteration < maxIterations)
            {
                iteration++;
                var responseContent = new System.Text.StringBuilder();
                AuthorRole? authorRole = null;
                var fccBuilder = new FunctionCallContentBuilder();

                // Stream the response
                await foreach (var streamingContent in _chatCompletionService.GetStreamingChatMessageContentsAsync(
                    chatHistory, 
                    executionSettings, 
                    _kernel))
                {
                    // Capture author role
                    authorRole ??= streamingContent.Role;

                    // Check metadata for thinking/reasoning content
                    if (streamingContent.Metadata != null)
                    {
                        // Check for thinking content
                        if (streamingContent.Metadata.TryGetValue("thinking", out var thinkingObj))
                        {
                            var thinkingText = thinkingObj?.ToString();
                            if (!string.IsNullOrWhiteSpace(thinkingText))
                            {
                                yield return new AiResponse
                                {
                                    ReplyText = thinkingText,
                                    Type = AiResponse.ResponseType.Thinking,
                                    Actions = new()
                                };
                            }
                        }

                        // Check for reasoning content
                        if (streamingContent.Metadata.TryGetValue("reasoning", out var reasoningObj))
                        {
                            var reasoningText = reasoningObj?.ToString();
                            if (!string.IsNullOrWhiteSpace(reasoningText))
                            {
                                yield return new AiResponse
                                {
                                    ReplyText = reasoningText,
                                    Type = AiResponse.ResponseType.Reasoning,
                                    Actions = new()
                                };
                            }
                        }
                    }

                    // Accumulate text content
                    if (!string.IsNullOrWhiteSpace(streamingContent.Content))
                    {
                        responseContent.Append(streamingContent.Content);
                        
                        yield return new AiResponse
                        {
                            ReplyText = streamingContent.Content,
                            Type = AiResponse.ResponseType.NormalResponse,
                            Actions = new()
                        };
                    }

                    // Collect function call details
                    fccBuilder.Append(streamingContent);
                }

                // Build the function calls from the streaming content
                IReadOnlyList<FunctionCallContent> functionCalls = fccBuilder.Build();
                
                if (!functionCalls.Any())
                {
                    // No function calls, we're done
                    continueLoop = false;
                }
                else
                {
                    // There were function calls - create message content to preserve them
                    ChatMessageContent fcContent = new ChatMessageContent(
                        role: authorRole ?? AuthorRole.Assistant, 
                        content: responseContent.ToString());
                    
                    // Add function calls to the message
                    foreach (var functionCall in functionCalls)
                    {
                        fcContent.Items.Add(functionCall);
                    }
                    
                    chatHistory.Add(fcContent);

                    // Execute each function call
                    foreach (var functionCall in functionCalls)
                    {
                        yield return new AiResponse
                        {
                            ReplyText = $"🔧 Calling tool: {functionCall.FunctionName}",
                            Type = AiResponse.ResponseType.ToolResponse,
                            Actions = new()
                        };

                        // Invoke the function
                        FunctionResultContent functionResult = await functionCall.InvokeAsync(_kernel);

                        // Add the function result to chat history
                        chatHistory.Add(functionResult.ToChatMessage());

                        yield return new AiResponse
                        {
                            ReplyText = $"✅ Tool '{functionCall.FunctionName}' executed successfully",
                            Type = AiResponse.ResponseType.ToolResponse,
                            Actions = new()
                        };
                    }
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
        }
    }
}
