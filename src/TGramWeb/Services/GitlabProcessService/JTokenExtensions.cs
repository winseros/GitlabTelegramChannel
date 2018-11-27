using System;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService
{
    internal static class JTokenExtensions
    {
        public static string RequireString(this JToken source, string key, JTokenErrors errors)
        {
            JToken data = source.RequireChild(key, errors);
            return data?.ToString();
        }

        public static JToken RequireChild(this JToken source, string key, JTokenErrors errors)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException(nameof(key));
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            JToken data = source[key];
            if (data == null)
            {
                errors.Add(string.IsNullOrEmpty(source.Path)
                               ? $"The json object is missing the field: \"{key}\""
                               : $"The json object is missing the field: \"{source.Path}.{key}\"");
            }

            return data;
        }
    }
}
