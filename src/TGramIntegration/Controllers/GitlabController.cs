using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramIntegration.Services.GitlabProcessService;

namespace TGramIntegration.Controllers
{
    [Route("api/gitlab_hook")]
    [ApiController]
    [Authorize]
    public class GitlabController : ControllerBase
    {
        private readonly IGitlabProcessService gitlabProcessService;
        private readonly ILogger logger;

        public GitlabController(IGitlabProcessService gitlabProcessService,
                                ILogger<GitlabController> logger)
        {
            this.gitlabProcessService = gitlabProcessService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Hook()
        {
            IActionResult result;
            using (var reader = new StreamReader(this.Request.Body))
            {
                string json = await reader.ReadToEndAsync();

                if (this.logger.IsEnabled(LogLevel.Trace))
                    this.logger.LogTrace(json);

                this.logger.LogDebug("Parsing and processing the incoming request");
                JObject obj = JObject.Parse(json);
                RequestProcessResult processResult = this.gitlabProcessService.ProcessRequest(obj);

                this.logger.LogDebug("The processing result was: {0}", processResult);
                result = processResult.Success 
                    ? (IActionResult)this.Ok() 
                    : this.BadRequest(processResult.Reason);
            }

            return result;
        }
    }
}
