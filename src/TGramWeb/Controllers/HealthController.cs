using Microsoft.AspNetCore.Mvc;

namespace TGramWeb.Controllers
{
    [Route("/")]
    [ApiController]
    public class HealthController: ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return this.Ok("Health check - Ok!");
        }
    }
}
