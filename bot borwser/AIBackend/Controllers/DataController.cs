using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AIBackend.Config;

namespace AIBackend.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly AppSettings _settings;

    public DataController(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("cards")]
    public IActionResult GetCards()
    {
        var baseUrl = _settings.BackendUrl;

        var cards = new[]
        {
            new { id = 1, title = "Corporate Tax", description = "GloBE minimum tax analysis", image = $"{baseUrl}/images/tax1.jpg" },
            new { id = 2, title = "Transfer Pricing", description = "Pricing compliance assistant", image = $"{baseUrl}/images/tax2.jpg" },
            new { id = 3, title = "AI Compliance Chat", description = "Talk with AI for guidance", image = $"{baseUrl}/images/tax3.jpg" },
            new { id = 4, title = "Generate Simple Idea", description = "Generate new simple idea via our platform", image = $"{baseUrl}/images/tax4.jpg" }
        };

        return Ok(cards);
    }

    [HttpGet("cards/{id}")]
    public IActionResult GetCardDetail(int id)
    {
        var baseUrl = _settings.BackendUrl;

        return Ok(new
        {
            id,
            title = $"Detail for Card #{id}",
            description = "Here the backend sends more details for the selected topic.",
            image = $"{baseUrl}/images/detail.jpg"
        });
    }
}