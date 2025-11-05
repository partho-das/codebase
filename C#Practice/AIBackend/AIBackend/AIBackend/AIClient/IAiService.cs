using AIBackend.Models;

namespace AIBackend.AIClient;

public interface IAiService 
{
    Task<AiResponse> AnalyzeAsync(AiRequest request);
}