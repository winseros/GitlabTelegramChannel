using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.MergeRequest
{
    public class MergeRequestMessageFormatter: IMergeRequestMessageFormatter
    {
        private readonly ILogger logger;

        public MergeRequestMessageFormatter(ILogger<MergeRequestMessageFormatter> logger)
        {
            this.logger = logger;
        }

        public RequestProcessResult TryFormat(JObject request, out string message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var errors = new JTokenErrors();

            JToken author = request.RequireChild(GitlabKeys.User, errors);
            JToken assignee = request.RequireChild(GitlabKeys.Assignee, errors);
            JToken project = request.RequireChild(GitlabKeys.Project, errors);
            JToken attributes = request.RequireChild(GitlabKeys.ObjectAttributes, errors);

            string authorName = author.RequireString(GitlabKeys.Name, errors);
            string assigneeName = assignee.RequireString(GitlabKeys.Name, errors);
            string projectName = project?.RequireString(GitlabKeys.Name, errors);
            string projectUrl = project?.RequireString(GitlabKeys.WebUrl, errors);

            string sourceBranch = attributes?.RequireString(GitlabKeys.SourceBranch, errors);
            string targetBranch = attributes?.RequireString(GitlabKeys.TargetBranch, errors);
            string state = attributes?.RequireString(GitlabKeys.State, errors);
            string title = attributes?.RequireString(GitlabKeys.Title, errors);
            string url = attributes?.RequireString(GitlabKeys.Url, errors);
            string iid = attributes?.RequireString(GitlabKeys.Iid, errors);
            string lastEditedAt = attributes?[GitlabKeys.LastEditedAt].ToString();

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
                state = MergeRequestMessageFormatter.GetMergeRequestState(state, lastEditedAt);
                message = $"[{projectName}]({projectUrl}). The merge request [#{iid} {title}]({url}) (branch [{sourceBranch}]({projectUrl}/tree/{sourceBranch}) to [{targetBranch}]({projectUrl}/tree/{targetBranch})) has been {state} by {authorName}.\r\nAssignee *{assigneeName}*.";
                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }

        private static string GetMergeRequestState(string state, string lastEditedAt)
        {
            string result = string.IsNullOrEmpty(lastEditedAt)
                                ? state
                                : string.Equals(state, GitlabKeys.StateOpened, StringComparison.InvariantCultureIgnoreCase)
                                    ? "updated"
                                    : state;
            return result;
        }
    }
}
