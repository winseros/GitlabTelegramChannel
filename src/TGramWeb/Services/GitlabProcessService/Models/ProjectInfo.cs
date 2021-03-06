using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.Models
{
    [DebuggerDisplay("Name: {Name}, Url: {Url}, Server: {Server}")]
    public class ProjectInfo
    {
        internal static ProjectInfo TryRead(JToken project, JTokenErrors errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            ProjectInfo result = null;
            if (project != null)
            {
                string projectName = project.RequireString(GitlabKeys.Name, errors);
                string projectUrl = project.RequireString(GitlabKeys.WebUrl, errors);
                string pathWithNamespace = project.RequireString(GitlabKeys.PathWithNamespace, errors);

                if (!errors.HasAny)
                {
                    string server = null;
                    if (projectUrl.EndsWith(pathWithNamespace))
                    {
                        server = projectUrl.Substring(0, projectUrl.Length - pathWithNamespace.Length);
                    }
                    else
                    {
                        errors.Add(project.Parent == null
                                       ? $"Can not retrieve the server url out of \"{GitlabKeys.WebUrl}\""
                                       : $"Can not retrieve the server url out of \"{project.Parent.Path}.{GitlabKeys.WebUrl}\"");
                    }
                
                    if (!errors.HasAny)
                    {
                        result = new ProjectInfo
                        {
                            Name = projectName,
                            Url = projectUrl,
                            Server = server
                        };
                    }
                }
            }

            return result;
        }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Server { get; set; }
    }
}
