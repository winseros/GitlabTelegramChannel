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
            JToken commit = request.RequireChild(GitlabKeys.Commit, errors);

            ProjectInfo projectInfo = project == null ? null : ProjectInfo.Read(project, errors);
            string branchName = attributes?.RequireString(GitlabKeys.Ref, errors);
            string pipelineId = attributes?.RequireString(GitlabKeys.Id, errors);
            CommitInfo commitInfo = commit == null ? null : CommitInfo.Read(commit, errors);

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
                message = $"[{projectInfo.Name}]({projectInfo.Url}). The pipeline [{pipelineId}]({projectInfo.Url}/pipelines/{pipelineId}) has failed for the branch [{branchName}]({projectInfo.Url}/tree/{branchName})!\r\n\r\n"
                          + $"The last commit [{commitInfo.Hash}]({commitInfo.Url}) by *{commitInfo.AuthorName}*\r\n{commitInfo.Message}";

                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }
    }
}
