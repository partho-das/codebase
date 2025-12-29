using AIBackend.Ai.Tools;
using AIBackend.Models;
using AIBackend.Services;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using System.Text.Json;

namespace AIBackend.AIClient
{
    public class OllamaChatClient : IAiService
    {
        private readonly string _model;
        private readonly OllamaChatModel _ollama;
        private readonly ToolRegistry _toolRegistry;

        public OllamaChatClient(IConfiguration cfg, ToolRegistry toolRegistry)
        {
            _toolRegistry = toolRegistry;
            _model = cfg["AI:Ollama:Model"] ?? "llama2";
            var baseUrl = cfg["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
            
            var provider = new OllamaProvider(baseUrl);
            _ollama = new OllamaChatModel(provider, _model);
        }

        public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
        {
            var systemPrompt = PromptHelpers.BasicBuildPrompt(request);
            var messages = new List<Message>
            {
                new Message(systemPrompt, MessageRole.System),
                new Message(request.Message, MessageRole.Human)
            };

            var chatRequest = ChatRequest.ToChatRequest(messages);
            var chatSettings = new OllamaChatSettings
            {
                UseStreaming = true,
                Temperature = 0.2f,
                NumPredict = 500
            };

            await foreach (var chunk in _ollama.GenerateAsync(chatRequest, chatSettings))
            {
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
}
