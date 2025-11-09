using System.Text.Json.Serialization;

namespace AIBackend.Models;

public class ActionCommand
{
    public CommandType Type { get; set; }
    public string Selector { get; set; } = ""; // CSS selector on the frontend

    public string Value { get; set; } = "";    // e.g. typed text or select value
    public int DurationMs { get; set; } = 600;

}

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum CommandType
{
    Move,
    Click,
    Type,
    Select,
    Scroll,
    Wait,
    ShowExplanation
}