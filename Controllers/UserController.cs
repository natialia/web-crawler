using Microsoft.AspNetCore.Mvc;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Swagger is working");
    }
}
