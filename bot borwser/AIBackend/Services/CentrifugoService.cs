using Centrifugo.AspNetCore.Abstractions;
using Centrifugo.AspNetCore.Configuration;
using Centrifugo.AspNetCore.Implementations;
using Centrifugo.AspNetCore.Models.Request;

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
        PublishParams publishParams = new PublishParams()
        {
            Channel = channel,
            Data = message
        };
        await _client.Publish(publishParams);
    }
}