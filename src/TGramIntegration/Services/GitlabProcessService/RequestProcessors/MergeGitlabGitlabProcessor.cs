using System;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService.RequestProcessors
{
    public class MergeGitlabGitlabProcessor: IGitlabProcessor
    {
        public RequestProcessResult Process(JObject request)
        {
            RequestProcessResult result;
            string objectKind = request["object_kind"]?.ToString();

            if (string.Equals(objectKind, "merge_request", StringComparison.InvariantCultureIgnoreCase))
            {
                result = RequestProcessResult.CreateSuccess();
            }
            else
            {
                result = RequestProcessResult.CreateNoResult();
            }

            return result;
        }
    }
}
