using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentSnippetMessageFormatter : ICommentSnippetMessageFormatter
    {
        private readonly ILogger logger;

        public CommentSnippetMessageFormatter(ILogger<CommentSnippetMessageFormatter> logger)
        {
            this.logger = logger;
        }

        public RequestProcessResult TryFormat(JObject request, out string message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var errors = new JTokenErrors();

            JToken author = request.RequireChild(GitlabKeys.User, errors);
            JToken project = request.RequireChild(GitlabKeys.Project, errors);
            JToken attributes = request.RequireChild(GitlabKeys.ObjectAttributes, errors);

            string authorName = author?.RequireString(GitlabKeys.Name, errors);
            string projectName = project?.RequireString(GitlabKeys.Name, errors);
            string projectUrl = project?.RequireString(GitlabKeys.WebUrl, errors);

            string snippetText = attributes?.RequireString(GitlabKeys.Note, errors);
            string snippetUrl = attributes?.RequireString(GitlabKeys.Url, errors);
            string createdAt = attributes?[GitlabKeys.CreatedAt]?.ToString();
            string updatedAt = attributes?[GitlabKeys.UpdatedAt]?.ToString();

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
                string commentAction = CommentSnippetMessageFormatter.GetCommentAction(createdAt, updatedAt);
                message = $"[{projectName}]({projectUrl}). *{authorName}* has [{commentAction}]({snippetUrl}) on the code snippet!\r\n\r\n{snippetText}";
                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }

        private static string GetCommentAction(string createdAt, string updatedAt)
        {
            return DateComparer.DateStringsMatch(createdAt, updatedAt)
                       ? "commented"
                       : "updated the comment";
        }
    }
}