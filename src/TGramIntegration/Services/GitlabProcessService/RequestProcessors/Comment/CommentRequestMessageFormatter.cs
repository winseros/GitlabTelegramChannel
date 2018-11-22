using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentRequestMessageFormatter : ICommentRequestMessageFormatter
    {
        private readonly ILogger logger;

        public CommentRequestMessageFormatter(ILogger<CommentRequestMessageFormatter> logger)
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
            JToken mergeRequest = request.RequireChild(GitlabKeys.MergeRequest, errors);

            string authorName = author?.RequireString(GitlabKeys.Name, errors);
            string projectName = project?.RequireString(GitlabKeys.Name, errors);
            string projectUrl = project?.RequireString(GitlabKeys.WebUrl, errors);

            string snippetText = attributes?.RequireString(GitlabKeys.Note, errors);
            string snippetUrl = attributes?.RequireString(GitlabKeys.Url, errors);
            string createdAt = attributes?[GitlabKeys.CreatedAt]?.ToString();
            string updatedAt = attributes?[GitlabKeys.UpdatedAt]?.ToString();

            string mrTitle = mergeRequest?.RequireString(GitlabKeys.Title, errors);
            string mrIid = mergeRequest?.RequireString(GitlabKeys.Iid, errors);

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
                string commentAction = CommentRequestMessageFormatter.GetCommentAction(createdAt, updatedAt);
                message = $"[{projectName}]({projectUrl}). *{authorName}* has [{commentAction}]({snippetUrl}) on the MR [#{mrIid} {mrTitle.Md()}]({projectUrl}/merge_requests/{mrIid})!\r\n\r\n{snippetText}";
                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }

        private static string GetCommentAction(string createdAt, string updatedAt)
        {
            return string.Equals(createdAt, updatedAt, StringComparison.CurrentCultureIgnoreCase)
                       ? "commented"
                       : "updated the comment";
        }
    }
}