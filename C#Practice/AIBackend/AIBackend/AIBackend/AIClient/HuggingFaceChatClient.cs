using System.Text;
using System.Text.Json;
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

    public HuggingFaceChatClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _apiKey = cfg["AI:HuggingFace:ApiKey"] ?? Environment.GetEnvironmentVariable("HF_API_KEY");
        _model = cfg["AI:HuggingFace:Model"] ?? "tiiuae/falcon-7b-instruct";
        if (string.IsNullOrEmpty(_apiKey))
            throw new ArgumentException("HuggingFace API key not configured. Set AI:HuggingFace:ApiKey or HF_API_KEY.");
    }

    public async Task<AiResponse> AnalyzeAsync(AiRequest request)
    {
        // Construct a prompt instructing the model to return strict JSON
        var prompt = PromptHelpers.BuildPrompt(request.Message);

        var body = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 200,
            temperature = 0.2
        };

        var json = JsonConvert.SerializeObject(body);
        var req = new HttpRequestMessage(HttpMethod.Post, $"https://router.huggingface.co/v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        req.Headers.Add("Authorization", $"Bearer {_apiKey}");

        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var str = await resp.Content.ReadAsStringAsync();

        // Hugging Face returns an array of dicts for some models; try to extract text
        // Many inference endpoints return: [{"generated_text":"..."}]
        string modelOutput = str;
        try
        {
            var arr = JsonConvert.DeserializeObject<dynamic>(str);
            if (arr is Newtonsoft.Json.Linq.JArray && arr.Count > 0 && arr[0]["generated_text"] != null)
            {
                modelOutput = (string)arr[0]["generated_text"];
            }
        }
        catch { /* fall back to raw text */ }

        return PromptHelpers.ParseModelOutput(modelOutput);
    }
}