

using AIBackend.Ai.Tools;
using AIBackend.Models;
using AIBackend.Services;
using CSharpToJsonSchema;
using LangChain.Providers;
using LangChain.Providers.Google;
using Newtonsoft.Json;

namespace AIBackend.AIClient;

public class LangChainChatClient : IAiService
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly GoogleChatModel _gemini;
    private readonly ToolRegistry _toolRegistry;

    public LangChainChatClient(IConfiguration cfg, ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
        _apiKey = cfg["AI:Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        _model = cfg["AI:Gemini:Model"] ?? "gemini-1.5-flash";
        if (string.IsNullOrEmpty(_apiKey))
            throw new ArgumentException("Google Gemini API key not configured. Set AI:Gemini:ApiKey or GEMINI_API_KEY.");

        var provider = new GoogleProvider(_apiKey, new HttpClient());
        if (provider.ChatSettings != null) provider.ChatSettings.UseStreaming = true;
        _gemini = new GoogleChatModel(provider, _model);
        
        // Register tools with LangChain
        var tools = _toolRegistry.AllTools.Select(t => new Tool
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.Schema
        }).ToList();
        
        var calls = _toolRegistry.AllTools.ToDictionary(
            t => t.Name,
            t => new Func<string, CancellationToken, Task<string>>(async (input, ct) =>
            {
                var result = await t.ExecuteAsync(input);
                return JsonConvert.SerializeObject(result);
            })
        );
        
        _gemini.AddGlobalTools(tools, calls);
    }

    public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
    {
        var systemPrompt = PromptHelpers.BasicBuildPrompt(request);
        var messages = new List<Message>
        {
            new Message(systemPrompt, MessageRole.System),
            new Message(request.Message, MessageRole.Human)
        };

        // Enable automatic tool handling
        _gemini.CallToolsAutomatically = true;
        _gemini.ReplyToToolCallsAutomatically = true;
        if (_gemini.Settings != null) _gemini.Settings.UseStreaming = true;

        var chatRequest = ChatRequest.ToChatRequest(messages);
        var chatSettings = ChatSettings.Default;
        chatSettings.UseStreaming = true;

        await foreach (var chunk in _gemini.GenerateAsync(chatRequest, chatSettings))
        {
            // Handle tool calls
            if (chunk.ToolCalls != null && chunk.ToolCalls.Count > 0)
            {
                foreach (var toolCall in chunk.ToolCalls)
                {
                    yield return new AiResponse
                    {
                        ReplyText = $"Calling tool: {toolCall.ToolName}...",
                        Type = AiResponse.ResponseType.NormalResponse,
                        Actions = new()
                    };

                    // Execute tool
                    var tool = _toolRegistry.GetTool(toolCall.ToolName);
                    if (tool != null)
                    {
                        var result = await tool.ExecuteAsync(toolCall.ToolArguments);
                        var resultJson = JsonConvert.SerializeObject(result);
                        
                        yield return new AiResponse
                        {
                            ReplyText = $"Tool result: {resultJson}",
                            Type = AiResponse.ResponseType.NormalResponse,
                            Actions = new()
                        };

                        // Add tool result to messages for next iteration
                        messages.Add(new Message(toolCall.ToolArguments, MessageRole.ToolCall, toolCall.ToolName));
                        messages.Add(new Message(resultJson, MessageRole.ToolResult, toolCall.ToolName));
                    }
                }
            }

            // Stream text content
            var text = chunk.LastMessageContent;
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return new AiResponse
                {
                    ReplyText = text,
                    Type = AiResponse.ResponseType.NormalResponse,
                    Actions = new()
                };
            }
        }
    }
}
