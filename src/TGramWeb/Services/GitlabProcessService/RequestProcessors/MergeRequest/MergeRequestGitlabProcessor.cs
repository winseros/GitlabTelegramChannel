using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.MergeRequest
{
    public class MergeRequestGitlabProcessor: IGitlabProcessor
    {
        private readonly IMergeRequestMessageFormatter formatter;
        private readonly IMessageClient messageClient;
        private readonly ILogger logger;

        public MergeRequestGitlabProcessor(IMergeRequestMessageFormatter formatter,
                                          IMessageClient messageClient,
                                          ILogger<MergeRequestGitlabProcessor> logger)
        {
            this.formatter = formatter;
            this.messageClient = messageClient;
            this.logger = logger;
        }

        public RequestProcessResult Process(JObject request)
        {
            RequestProcessResult result;
            string objectKind = request[GitlabKeys.ObjectKind]?.ToString();
            this.logger.LogTrace("The request object kind was determined as: \"{0}\"", objectKind);

            if (string.Equals(objectKind, GitlabKeys.ObjectKindMergeRequest, StringComparison.InvariantCultureIgnoreCase))
            {
                result = this.formatter.TryFormat(request, out string message);
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
                this.logger.LogTrace("Can not handle the request of the \"{0}\" object kind", objectKind);
                result = RequestProcessResult.CreateNoResult();
            }

            return result;
        }
    }
}
