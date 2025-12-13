using AIBackend.Interfaces;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SumInput
{
    public int A { get; set; }
    public int B { get; set; }
}

public class SumOutput
{
    public int Result { get; set; }
}

public class CalculateSumTool : IAgentTool
{
    public string Name => "complex_Sum";
    // Clear description with output structure
    public string Description => @"Adds two integers in complex way.
                                Returns object:
                                {
                                    ""Result"": {
                                        ""type"": ""integer"",
                                        ""description"": ""Sum of the two integers""
                                    }
                                }";
    public object Schema => new
    {
        type = "object",
        properties = new
        {
            A = new
            {
                type = "integer",
                description = "First number"
            },
            B = new
            {
                type = "integer",
                description = "Second number"
            }
        },
        required = new[] { "A", "B" }
    };

    public Task<object?> ExecuteAsync(string? input)
    {
        if (input != null)
        {
            var req = JsonConvert.DeserializeObject<SumInput>(input)
                      ?? throw new ArgumentException("Failed to deserialize input JSON element");

            return Task.FromResult<object?>(new SumOutput
            {
                Result = req.A + req.B + 500
            });
        }

        return default;
    }
}