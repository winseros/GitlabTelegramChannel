using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService.Models;

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

            JToken authorNode = request.RequireChild(GitlabKeys.User, errors);
            JToken assigneeNode = request.RequireChild(GitlabKeys.Assignee, errors);
            JToken projectNode = request.RequireChild(GitlabKeys.Project, errors);
            JToken attributesNode = request.RequireChild(GitlabKeys.ObjectAttributes, errors);

            UserInfo author = UserInfo.TryRead(authorNode, errors);
            UserInfo assignee = UserInfo.TryRead(assigneeNode, errors);
            ProjectInfo projectInfo = ProjectInfo.TryRead(projectNode, errors);

            string sourceBranch = attributesNode?.RequireString(GitlabKeys.SourceBranch, errors);
            string targetBranch = attributesNode?.RequireString(GitlabKeys.TargetBranch, errors);
            string state = attributesNode?.RequireString(GitlabKeys.State, errors);
            string title = attributesNode?.RequireString(GitlabKeys.Title, errors);
            string url = attributesNode?.RequireString(GitlabKeys.Url, errors);
            string iid = attributesNode?.RequireString(GitlabKeys.Iid, errors);
            string createdAt = attributesNode?[GitlabKeys.CreatedAt]?.ToString();
            string updatedAt = attributesNode?[GitlabKeys.UpdatedAt]?.ToString();

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
                state = MergeRequestMessageFormatter.GetMergeRequestState(state, createdAt, updatedAt);
                message = $"[{projectInfo.Name}]({projectInfo.Url}). The merge request [#{iid} {title}]({url}) (branch [{sourceBranch}]({projectInfo.Url}/tree/{sourceBranch}) to [{targetBranch}]({projectInfo.Url}/tree/{targetBranch})) was {state} by [{author.Name}]({projectInfo.Server}{author.UserName}).\r\nAssignee [{assignee.Name}]({projectInfo.Server}{assignee.UserName}).";
                this.logger.LogTrace("Composed the message: \"{0}\"", message);
                result = RequestProcessResult.CreateSuccess();
            }

            return result;
        }

        private static string GetMergeRequestState(string state, string createdAt, string updatedAt)
        {
            string result = DateComparer.DateStringsMatch(createdAt, updatedAt) //they might differ by 1 second
                                ? state
                                : string.Equals(state, GitlabKeys.StateOpened, StringComparison.InvariantCultureIgnoreCase)
                                    ? "updated"
                                    : state;
            return result;
        }
    }
}
