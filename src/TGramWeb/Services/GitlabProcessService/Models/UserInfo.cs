using System;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.Models
{
    public class UserInfo
    {
        internal static UserInfo TryRead(JToken user, JTokenErrors errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            UserInfo result = null;
            if (user != null)
            {
                string name = user.RequireString(GitlabKeys.Name, errors);
                string username = user.RequireString(GitlabKeys.UserName, errors);

                if (!errors.HasAny)
                {
                    result = new UserInfo
                    {
                        Name = name,
                        UserName = username
                    };
                }
            }

            return result;
        }

        public string Name { get; set; }

        public string UserName { get; set; }
    }
}
