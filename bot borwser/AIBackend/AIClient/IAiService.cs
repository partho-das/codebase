using AIBackend.Models;

namespace AIBackend.AIClient;

public interface IAiService 
{
    IAsyncEnumerable<AiResponse> AnalyzeAsync(AiRequest request);
}