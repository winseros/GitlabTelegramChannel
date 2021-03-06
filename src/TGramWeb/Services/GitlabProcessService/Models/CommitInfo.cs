using System;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.Models
{
    public class CommitInfo
    {
        internal static CommitInfo TryRead(JToken commit, JTokenErrors errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            CommitInfo result = null;
            if (commit != null)
            {
                string id = commit.RequireString(GitlabKeys.Id, errors);
                string message = commit.RequireString(GitlabKeys.Message, errors);
                string url = commit.RequireString(GitlabKeys.Url, errors);

                JToken author = commit.RequireChild(GitlabKeys.Author, errors);
                string authorName = author?.RequireString(GitlabKeys.Name, errors);
                string authorEmail = author?.RequireString(GitlabKeys.Email, errors);

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
