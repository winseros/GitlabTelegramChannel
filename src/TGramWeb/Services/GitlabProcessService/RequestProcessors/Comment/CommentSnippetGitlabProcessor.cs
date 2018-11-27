using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.MessageClient;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentSnippetGitlabProcessor: CommentGitlabProcessor
    {
        private readonly ICommentSnippetMessageFormatter formatter;

        public CommentSnippetGitlabProcessor(ICommentSnippetMessageFormatter formatter,
                                             IMessageClient messageClient,
                                             ILogger<CommentSnippetGitlabProcessor> logger)
            : base(messageClient, logger)
        {
            this.formatter = formatter;
        }

        protected override bool CanHandle(string noteableType)
        {
            return string.Equals(noteableType, GitlabKeys.NoteableTypeSnippet, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override RequestProcessResult TryFormat(JObject request, out string message)
        {
            RequestProcessResult result = this.formatter.TryFormat(request, out message);
            return result;
        }
    }
}
