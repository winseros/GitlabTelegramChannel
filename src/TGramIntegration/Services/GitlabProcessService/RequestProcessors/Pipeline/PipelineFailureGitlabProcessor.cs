using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline
{
    public class PipelineFailureGitlabProcessor: IGitlabProcessor
    {
        private readonly IPipelineMessageFormatter messageFormatter;
        private readonly IMessageClient messageClient;
        private readonly ILogger logger;

        public PipelineFailureGitlabProcessor(IPipelineMessageFormatter messageFormatter,
                                              IMessageClient messageClient,
                                              ILogger<PipelineFailureGitlabProcessor> logger)
        {
            this.messageFormatter = messageFormatter;
            this.messageClient = messageClient;
            this.logger = logger;
        }

        public RequestProcessResult Process(JObject request)
        {
            RequestProcessResult result;

            string objectKind = request[GitlabKeys.ObjectKind]?.ToString();
            this.logger.LogTrace("The request object kind was determined as: \"{0}\"", objectKind);

            if (string.Equals(objectKind, GitlabKeys.ObjectKindPipeline, StringComparison.InvariantCultureIgnoreCase))
            {
                var errors = new JTokenErrors();

                string requestStatus = request.RequireChild(GitlabKeys.ObjectAttributes, errors)?.RequireString(GitlabKeys.Status, errors);
                this.logger.LogDebug("The request status was determined as \"{0}\"", requestStatus);

                if (errors.HasAny)
                {
                    string error = errors.Compose();
                    result = RequestProcessResult.CreateFailure(error);
                    this.logger.LogDebug("The request processing was rejected with message: \"{0}\"", error);
                }
                else
                {
                    if (string.Equals(requestStatus, GitlabKeys.StatusFailure, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = this.messageFormatter.TryFormat(request, out string message);
                        if (result.Success)
                        {
                            this.logger.LogDebug("Successfully formatted the message: \"{0}\"", message);
                            this.messageClient.ScheduleDelivery(message);
                        }
                        else
                        {
                            this.logger.LogDebug("Could not format the message: {@0}", result);
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
