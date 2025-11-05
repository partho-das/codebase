using AIBackend.Models;
using Newtonsoft.Json;

namespace AIBackend.Services
{
    public static  class PromptHelpers
    {
        public static string BuildPrompt(string userMessage)
        { 
            var instructions = @"
                You are an assistant for a web UI (Orbitax tax calculator). The user asked: 
                """ + userMessage + @"""
                You MUST output valid JSON only (no extra text). The JSON must have:
                {
                  ""reply"": ""<text for chat UI (short)>"",
                  ""actions"": [
                    {
                      ""type"": ""move|click|type|select|scroll|showExplanation"",
                      ""selector"": ""<css selector on page, optional for showExplanation>"",
                      ""value"": ""<text to type or value to select>"",
                      ""durationMs"": <integer, optional>
                    }
                  ]
                }
                Rules:
                - Output only JSON, nothing else.
                - Keep reply short (1-2 sentences).
                - Use selectors that match the target UI (e.g. '#country-select', '#revenue-input', '#calculate').
                - If no UI action is needed, return an empty actions array.
                Example:
                { ""reply"": ""Filling Germany and calculating."",
                  ""actions"": [{""type"":""select"",""selector"":""#country-select"",""value"":""Germany""},
                                {""type"":""type"",""selector"":""#revenue-input"",""value"":""1200000""},
                                {""type"":""click"",""selector"":""#calculate""}]
                }
                Now produce the JSON output for the user's request above.";

            return instructions;
        }

        // Parse a model response (string) into AIResponse.
        public static AiResponse ParseModelOutput(string modelOutput)
        {
            // Models might wrap JSON in markdown or extra text; try to extract first JSON object in text.
            var json = ExtractFirstJson(modelOutput);
            if (string.IsNullOrEmpty(json))
            {
                // Fallback: put full model output in reply and no actions
                return new AiResponse { ReplyText = modelOutput ?? "", Actions = new List<ActionCommand>() };
            }

            try
            {
                var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);

                var res = new AiResponse
                {
                    ReplyText = jsonObj?.reply != null ? (string)jsonObj.reply : ""
                };

                if (jsonObj?.actions != null)
                {
                    foreach (var a in jsonObj.actions)
                    {
                        var cmd = new ActionCommand
                        {
                            Type = a.type != null ? (CommandType)a.type : CommandType.ShowExplanation,
                            Selector = a.selector != null ? (string)a.selector : "",
                            Value = a.value != null ? (string)a.value : "",
                            DurationMs = a.durationMs != null ? (int)a.durationMs : 0
                        };
                        res.Actions.Add(cmd);
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                // parsing failed
                return new AiResponse { ReplyText = modelOutput ?? "", Actions = new List<ActionCommand>() };
            }
        }

        // Attempt to find first JSON object in a string
        private static string ExtractFirstJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null!;
            int start = text.IndexOf('{');
            if (start < 0) return null!;
            int depth = 0;
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}') depth--;
                if (depth == 0)
                {
                    return text.Substring(start, i - start + 1);
                }
            }
            return null!;
        }
    }
}
