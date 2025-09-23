using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Saga_State_Machine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributeController : ControllerBase
    {
        [HttpPost("Command")]
        public async Task<IActionResult> CommandHandler([FromBody] string command)
        {
            return Ok(new { Message = "Command distributed successfully", Command = command });
        }
    }
}
