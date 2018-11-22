using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment
{
    public abstract class CommentGitlabProcessor: IGitlabProcessor
    {
        private readonly IMessageClient messageClient;
        private readonly ILogger logger;

        protected CommentGitlabProcessor(IMessageClient messageClient,
                                         ILogger logger)
        {
            this.messageClient = messageClient;
            this.logger = logger;
        }

        public RequestProcessResult Process(JObject request)
        {
            RequestProcessResult result;

            string objectKind = request[GitlabKeys.ObjectKind]?.ToString();
            this.logger.LogTrace("The request object kind was determined as: \"{0}\"", objectKind);

            if (string.Equals(objectKind, GitlabKeys.ObjectKindNote, StringComparison.InvariantCultureIgnoreCase))
            {
                var errors = new JTokenErrors();

                string noteableType = request.RequireChild(GitlabKeys.ObjectAttributes, errors)?.RequireString(GitlabKeys.NoteableType, errors);
                this.logger.LogDebug("The noteable type was determined as \"{0}\"", noteableType);

                if (errors.HasAny)
                {
                    string error = errors.Compose();
                    result = RequestProcessResult.CreateFailure(error);
                    this.logger.LogDebug("The request processing was rejected with message: \"{0}\"", error);
                }
                else
                {
                    if (this.CanHandle(noteableType))
                    {
                        result = this.TryFormat(request, out string message);
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
                        this.logger.LogDebug("Can not handle the request with the \"{0}\" noteable type", noteableType);
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

        protected abstract bool CanHandle(string noteableType);

        protected abstract RequestProcessResult TryFormat(JObject request, out string message);
    }
}
