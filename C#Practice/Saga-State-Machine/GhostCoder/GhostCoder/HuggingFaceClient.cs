using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GhostCoder;

public class HuggingFaceChatClient
{
    private readonly HttpClient _http;

    public HuggingFaceChatClient(string token)
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    /// <summary>
    /// Sends a user message to the Hugging Face chat model and returns assistant content.
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string model = "meta-llama/Llama-3.2-3B-Instruct:novita")
    {
        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            },
            max_tokens = 50,
            temperature = 0.2
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.PostAsync(
            "https://router.huggingface.co/v1/chat/completions",
            content
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HF API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);

        // Extract the assistant's reply
        var choice = doc.RootElement.GetProperty("choices")[0];
        var assistantMessage = choice.GetProperty("message").GetProperty("content").GetString();

        return assistantMessage;
    }
}