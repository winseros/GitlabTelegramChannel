using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline
{
    public class PipelineMessageFormatter: IPipelineMessageFormatter
    {
        private readonly ILogger logger;

        public PipelineMessageFormatter(ILogger<PipelineMessageFormatter> logger)
        {
            this.logger = logger;
        }

        public RequestProcessResult TryFormat(JObject request, out string message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var errors = new JTokenErrors();

            JToken project = request.RequireChild(GitlabKeys.Project, errors);
            JToken attributes = request.RequireChild(GitlabKeys.ObjectAttributes, errors);

            string projectName = project?.RequireString(GitlabKeys.Name, errors);
            string projectUrl = project?.RequireString(GitlabKeys.WebUrl, errors);
            string branchName = attributes?.RequireString(GitlabKeys.Ref, errors);
            string pipelineId = attributes?.RequireString(GitlabKeys.Id, errors);

            RequestProcessResult result;

            if (errors.HasAny)
            {
                string error = errors.Compose();
                this.logger.LogDebug("The request processing was rejected with error: \"{0}\"", error);

                message = null;
                result = RequestProcessResult.CreateFailure(error);
            }
            else
            {
                message = $"[{projectName}]({projectUrl}). The pipeline [{pipelineId}]({projectUrl}/pipelines/{pipelineId}) has failed for the branch [{branchName}]({projectUrl}/tree/{branchName})!";
                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }
    }
}
