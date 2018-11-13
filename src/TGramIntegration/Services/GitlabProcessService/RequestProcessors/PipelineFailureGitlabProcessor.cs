using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors
{
    public class PipelineFailureGitlabProcessor: IGitlabProcessor
    {
        private readonly IMessageClient messageClient;
        private readonly ILogger logger;

        public PipelineFailureGitlabProcessor(IMessageClient messageClient,
                                              ILogger<PipelineFailureGitlabProcessor> logger)
        {
            this.messageClient = messageClient;
            this.logger = logger;
        }

        public RequestProcessResult Process(JObject request)
        {
            RequestProcessResult result;

            string objectKind = request["object_kind"]?.ToString();
            this.logger.LogTrace("The request object kind was determined as: \"{0}\"", objectKind);

            if (string.Equals(objectKind, "pipeline", StringComparison.InvariantCultureIgnoreCase))
            {
                var errors = new JTokenErrors();

                string requestStatus = request.RequireChild("object_attributes", errors)?.RequireString("status", errors);
                this.logger.LogDebug("The request status was determined as \"{0}\"", requestStatus);

                if (errors.HasAny)
                {
                    string error = errors.Compose();
                    result = RequestProcessResult.CreateFailure(error);
                    this.logger.LogDebug("The request processing was rejected with message: \"{0}\"", error);
                }
                else
                {
                    if (string.Equals(requestStatus, "failure", StringComparison.InvariantCultureIgnoreCase))
                    {
                        JToken project = request.RequireChild("project", errors);
                        JToken attributes = request.RequireChild("object_attributes", errors);

                        string projectName = project?.RequireString("name", errors);
                        string projectUrl = project?.RequireString("web_url", errors);
                        string branchName = attributes?.RequireString("ref", errors);
                        string pipelineId = attributes?.RequireString("id", errors);

                        if (errors.HasAny)
                        {
                            string error = errors.Compose();
                            result = RequestProcessResult.CreateFailure(error);
                            this.logger.LogDebug("The request processing was rejected with message: \"{0}\"", error);
                        }
                        else
                        {
                            string message = $"*{projectName}*\r\nThe pipeline [#{pipelineId}]({projectUrl}/pipelines/{pipelineId}) has failed for the branch *{branchName}*!";
                            this.logger.LogTrace("Composed the message: \"{0}\"", message);

                            this.messageClient.ScheduleDelivery(message);

                            result = RequestProcessResult.CreateSuccess();
                        }
                    }
                    else
                    {
                        this.logger.LogDebug("Can not handle the request with the \"{0}\" status", requestStatus);
                        result = RequestProcessResult.CreateNoResult();
                    }
                }
            }
            else
            {
                this.logger.LogTrace("Can not handle the request of the \"{0}\" object kind", objectKind);
                result = RequestProcessResult.CreateNoResult();
            }
            

            return result;
        }
    }
}
