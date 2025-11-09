using AIBackend.Models;
using Newtonsoft.Json;

namespace AIBackend.Services
{
    public static  class PromptHelpers
    {
        public static string BuildPrompt(AiRequest userMessage)
        {
            var instructions = $@"
                        You are an AI assistant that controls a dynamic web UI.
                        The user said:
                        \""{ userMessage.Message}\""

                        Here is the current snapshot of the page’s DOM (this may change frequently):
                        \""{ userMessage.SnapshotString}\""

                        -----------Instructions-----------
                        You must respond with **valid JSON only** (no extra text, no markdown).

                        JSON format:
                        {{
                          ""reply"": ""<short user-facing message (1–2 sentences)>"",
                          ""actions"": [
                            {{
                              ""type"": ""click | type | select | scroll | move | wait | showExplanation"",
                              ""selector"": ""<CSS selector identifying the element on the current page>"",
                              ""value"": ""<text or selection value if needed>"",
                              ""durationMs"": <integer, optional for move/scroll/wait>
                            }}
                          ]
                        }}

                        Rules:
                        - Use the provided DOM snapshot to infer selectors (e.g. use element IDs, text labels, or visible attributes).
                        - Only include actions that exist in the current UI.
                        - If you can’t find a clear element, skip that action (never hallucinate selectors).
                        - If the user request is informational (no UI action), return an empty `actions` array.
                        - Prefer stable selectors (IDs, data attributes, clear class names).
                        - Combine multiple user intents into sequential actions when needed.
                        - Never output anything other than the JSON.

                        Example (purely illustrative — do not rely on these IDs):
                        {{
                          ""reply"": ""Setting country and calculating."",
                          ""actions"": [
                            {{ ""type"": ""select"", ""selector"": ""#country-dropdown"", ""value"": ""Germany"" }},
                            {{ ""type"": ""type"", ""selector"": ""#revenue-input"", ""value"": ""1200000"" }},
                            {{ ""type"": ""click"", ""selector"": ""#calculate"" }}
                          ]
                        }}
                        ";

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
