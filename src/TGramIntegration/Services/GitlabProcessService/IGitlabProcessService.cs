using Newtonsoft.Json.Linq;

namespace TGramIntegration.Services.GitlabProcessService
{
    public interface IGitlabProcessService
    {
        RequestProcessResult ProcessRequest(JObject request);
    }
}
