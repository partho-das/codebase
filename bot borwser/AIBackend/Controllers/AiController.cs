using AIBackend.AIClient;
using AIBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIBackend.Controllers;


[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    public readonly IAiService _ai;

    public AiController(IAiService ai)
    {
        _ai = ai;
    }

    [HttpPost("agent")]
    public async Task<IActionResult> Agent([FromBody] AiRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("message required");
        var res = await _ai.AnalyzeAsync(req);
        return Ok(res);
    }
}