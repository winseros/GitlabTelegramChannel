using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService.Models;

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

            JToken projectNode = request.RequireChild(GitlabKeys.Project, errors);
            JToken attributesNode = request.RequireChild(GitlabKeys.ObjectAttributes, errors);
            JToken commitNode = request.RequireChild(GitlabKeys.Commit, errors);

            ProjectInfo project = ProjectInfo.TryRead(projectNode, errors);
            string branchName = attributesNode?.RequireString(GitlabKeys.Ref, errors);
            string pipelineId = attributesNode?.RequireString(GitlabKeys.Id, errors);
            CommitInfo commit = CommitInfo.TryRead(commitNode, errors);

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
                message = $"[{project.Name}]({project.Url}). The pipeline [{pipelineId}]({project.Url}/pipelines/{pipelineId}) has failed for the branch [{branchName}]({project.Url}/tree/{branchName})!\r\n\r\n"
                          + $"The last commit [{commit.Hash}]({commit.Url}) by *{commit.AuthorName}*\r\n{commit.Message}";

                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }
    }
}
