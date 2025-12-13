using System.Text;
using System.Text.Json;
using AIBackend.Models;
using AIBackend.Services;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AIBackend.AIClient
{
    public class OllamaChatClient : IAiService
    {
        public readonly HttpClient _http;
        public readonly string _model;
        public readonly string _baseUrl;

        public OllamaChatClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _model = cfg["AI:Ollama:Model"] ?? "llama2";
            _baseUrl = cfg["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
        }
        public async IAsyncEnumerable<AiResponse?> AnalyzeAsync(AiRequest request)
        {
            var prompt = PromptHelpers.BasicBuildPrompt(request);

            var body = new
            {
                model = _model,
                prompt,
                temperature = 0.2,
                max_tokens = 200,
                stream = true
            };
            var jsonBody = JsonConvert.SerializeObject(body);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/generate")
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            using var response = await _http.SendAsync(req);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var read = new StreamReader(stream, Encoding.UTF8);

            string returnValueTask = "";

        
                while (!read.EndOfStream)
                {
                    var line = await read.ReadLineAsync();
                    if (line == null) break;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("data: "))
                    {
                        var jsonPart = line.Substring("data: ".Length);
                        if (jsonPart == "[DONE]") break;
                        var chunkObj = JsonDocument.Parse(jsonPart);
                        returnValueTask += chunkObj.RootElement.GetProperty("choices")[0].GetProperty("delta").GetString();
                        Console.Write(returnValueTask);
                    }
                    else
                    {
                        var jsonDocument = JsonDocument.Parse(line);
                        returnValueTask += jsonDocument.RootElement.ToString();


                }
                //yield return new AiResponse { ReplyText = returnValueTask, Actions = new List<ActionCommand>() };

                }

                yield return new AiResponse { ReplyText = returnValueTask, Actions = new List<ActionCommand>() };

        }
    }
}
