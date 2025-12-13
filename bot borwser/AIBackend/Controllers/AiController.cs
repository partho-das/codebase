using System.Text.Json;
using AIBackend.AIClient;
using AIBackend.Interfaces;
using AIBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AIBackend.Controllers;


[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    public readonly IAiService _ai;
    public readonly IAiSessionStore _aiSessionStore;
    public AiController(IAiService ai, IAiSessionStore aiSessionStore)
    {
        _ai = ai;
        _aiSessionStore = aiSessionStore;
    }

    [HttpPost("agent")]
    public async Task<IActionResult> Agent([FromBody] AiRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("message required");
        //var res = await _ai.AnalyzeAsync(req);
        return Ok("hi");
    }

    [HttpPost("chat")]

    public async Task<IActionResult> Chat([FromBody] AiRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Message) || !_aiSessionStore.CreateSession(req.Id, req))
            return BadRequest("message required");
        return Ok(new {SessionId = req.Id});
    }


    [HttpGet("stream")]
    public async Task<IActionResult> Stream([FromQuery] string sessionId, CancellationToken ct)
    {
        var req = _aiSessionStore.GetSession(sessionId);
        if (req == null || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("Invalid Session!");
        Response.Headers.Add("Content-Type", "text/event-stream");

        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        await foreach (var chunk in _ai.AnalyzeAsync(req).WithCancellation(ct))
        {
            if(chunk == null || string.IsNullOrEmpty(chunk.ReplyText))
                continue;

            var json = JsonSerializer.Serialize(chunk);

            Console.WriteLine($"chunk:<^>{chunk.ReplyText}<^>");

            // SSE format
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);


            await Task.Delay(50, ct);
            if (ct.IsCancellationRequested)
                break;

        }


        await Response.WriteAsync("data: {\"ResponseDone\": true}\n\n", ct);
        await Response.Body.FlushAsync(ct);

        _aiSessionStore.DeleteSession(sessionId);
        return new EmptyResult();
    }

    [HttpGet("testStream")]
    public async IAsyncEnumerable<string> StreamData()
    {
        for (int i = 1; i <= 10; i++)
        {
            yield return $"Chunk {i}\n";
            await Task.Delay(1000);
        }
    }

}