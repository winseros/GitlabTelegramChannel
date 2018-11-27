using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentRequestGitlabProcessor: CommentGitlabProcessor
    {
        private readonly ICommentRequestMessageFormatter formatter;

        public CommentRequestGitlabProcessor(ICommentRequestMessageFormatter formatter,
                                             IMessageClient messageClient,
                                             ILogger<CommentRequestGitlabProcessor> logger)
            : base(messageClient, logger)
        {
            this.formatter = formatter;
        }

        protected override bool CanHandle(string noteableType)
        {
            return string.Equals(noteableType, GitlabKeys.NoteableTypeMergeRequest, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override RequestProcessResult TryFormat(JObject request, out string message)
        {
            RequestProcessResult result = this.formatter.TryFormat(request, out message);
            return result;
        }
    }
}
