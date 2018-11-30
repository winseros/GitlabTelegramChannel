using Microsoft.AspNetCore.Mvc;

namespace TGramWeb.Controllers
{
    [Route("/health")]
    [ApiController]
    public class HealthController: ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return this.Ok();
        }
    }
}
