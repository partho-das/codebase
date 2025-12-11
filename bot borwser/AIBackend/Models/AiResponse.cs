namespace AIBackend.Models;

public class AiResponse
{
    public enum ResponseType
    {
        Reasoning,      // e.g., AI explains or plans
        Thinking,       // e.g., partial streaming text
        NormalResponse  // e.g., final message text
    }

    public string? ReplyText { get; set; } = "";
    public List<ActionCommand> Actions { get; set; } = new List<ActionCommand>();
    public ResponseType Type { get; set; } = ResponseType.NormalResponse;
    
}