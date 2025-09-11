using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferrio.EntityMap.Prototype.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
