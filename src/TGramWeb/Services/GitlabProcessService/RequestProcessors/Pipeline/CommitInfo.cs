using System;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline
{
    public class CommitInfo
    {
        internal static CommitInfo Read(JToken commit, JTokenErrors errors)
        {
            if (commit == null)
                throw new ArgumentNullException(nameof(commit));
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            string id = commit.RequireString(GitlabKeys.Id, errors);
            string message = commit.RequireString(GitlabKeys.Message, errors);
            string url = commit.RequireString(GitlabKeys.Url, errors);

            JToken author = commit.RequireChild(GitlabKeys.Author, errors);
            string authorName = author?.RequireString(GitlabKeys.Name, errors);
            string authorEmail = author?.RequireString(GitlabKeys.Email, errors);

            CommitInfo result = null;
            if (!errors.HasAny)
            {
                result = new CommitInfo
                {
                    Hash = id.Substring(0, 7),
                    AuthorName = authorName,
                    AuthorEmail = authorEmail,
                    Message = message,
                    Url = url
                };
            }

            return result;
        }

        public string Hash { get; set; }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public string Message { set; get; }

        public string Url { get; set; }
    }
}
