using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService
{
    public interface IGitlabProcessService
    {
        RequestProcessResult ProcessRequest(JObject request);
    }
}
