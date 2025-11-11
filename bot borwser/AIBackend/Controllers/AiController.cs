using AIBackend.AIClient;
using AIBackend.Models;
using AIBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIBackend.Controllers;


[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;
    private readonly CentrifugoService _centrifugo;

    public AiController(IAiService ai, CentrifugoService centrifugo)
    {
        _ai = ai;
        _centrifugo = centrifugo;
    }

    [HttpPost("agent")]
    public async Task<IActionResult> Agent([FromBody] AiRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("message required");
        var res = await _ai.AnalyzeAsync(req);
        await _centrifugo.PublishAsync("partho_ai_chat", res);
        return Ok(res);
    }
}