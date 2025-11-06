namespace AIBackend.Models;

public class ActionCommand
{
    public CommandType Type { get; set; }
    public string Selector { get; set; } = ""; // CSS selector on the frontend

    public string Value { get; set; } = "";    // e.g. typed text or select value
    public int DurationMs { get; set; } = 600;

}
public enum CommandType
{
    Move,
    Click,
    Type,
    Select,
    Scroll,
    ShowExplanation
}