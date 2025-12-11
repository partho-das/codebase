using System;
using System.Threading.Tasks;
using AIBackend.Interfaces;
using AIBackend.Models;
using Newtonsoft.Json;

namespace AIBackend.Ai.Tools
{
    public class GetWeatherInput
    {
        public string City { get; set; } = string.Empty;
    }

    public class GetWeatherOutput
    {
        public string City { get; set; } = string.Empty;
        public double TemperatureCelsius { get; set; }
        public string Condition { get; set; } = string.Empty;
    }

    public class GetWeatherTool : IAgentTool
    {
        public string Name => "get_weather";

        public string Description => @"Returns current weather for a city. 
                                        Returns object:
                                        {
                                            ""City"": {
                                                ""type"": ""string"",
                                                ""description"": ""City name""
                                            },
                                            ""TemperatureCelsius"": {
                                                ""type"": ""number"",
                                                ""description"": ""Current temperature in Celsius""
                                            },
                                            ""Condition"": {
                                                ""type"": ""string"",
                                                ""description"": ""Weather condition (Sunny, Rainy, etc.)""
                                            }
                                        }";
        // JSON schema describing input/output
            public object Schema => new
            {
                type = "object",
                properties = new
                {
                    City = new
                    {
                        type = "string",
                        description = "Name of the city to get weather for"
                    }
                },
                required = new[] { "City" }
            };




        public Task<object?> ExecuteAsync(string? input)
        {
            if (input == null) return default;


            var req = JsonConvert.DeserializeObject<GetWeatherInput>(input);
            if (req == null) return default;

            return Task.FromResult<object?>(new GetWeatherOutput
            {
                City = req.City,
                TemperatureCelsius = 22, // dummy value
                Condition = "Sunny"
            });
        }
    }
}