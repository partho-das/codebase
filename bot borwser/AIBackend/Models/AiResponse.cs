using System.Text.Json.Serialization;

namespace AIBackend.Models;

public class AiResponse
{

    public string ReplyText { get; set; } = "";
    public List<ActionCommand> Actions { get; set; } = new List<ActionCommand>();
}