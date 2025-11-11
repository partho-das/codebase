using Centrifugo.AspNetCore.Abstractions;
using Centrifugo.AspNetCore.Configuration;
using Centrifugo.AspNetCore.Implementations;
using Centrifugo.AspNetCore.Models.Request;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIBackend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AIBackend.Services;

public class CentrifugoService
{
    private readonly ICentrifugoClient _client;
    private readonly string _apiUrl;
    private readonly string _apiKey;


    public CentrifugoService(IConfiguration cfg)
    {
        _apiUrl = cfg["Centrifugo:ApiUrl"] ?? "1234";
        _apiKey = cfg["Centrifugo:ApiKey"] ?? "tiiuae/falcon-7b-instruct";

        _client = new CentrifugoClient(new CentrifugoOptions
        {
            Url = _apiUrl,
            ApiKey = _apiKey
        });
    }

    public async Task PublishAsync(string channel, object message)
    {
  
        string jsonContent = JsonConvert.SerializeObject(message, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.None
        });


        var publishParams = new PublishParams
        {
            Channel = channel,
            Data = jsonContent
        };


        var response = await _client.Publish(publishParams);

        if (!response.IsSuccess)
        {
            Console.WriteLine($"Centrifugo publish failed: {response.IsSuccess} - {response.Result}");
        }
        else
        {
            Console.WriteLine($"Published to {channel}");
        }

    }
}