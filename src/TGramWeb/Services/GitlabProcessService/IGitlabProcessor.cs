using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService
{
    public interface IGitlabProcessor
    {
        RequestProcessResult Process(JObject request);
    }
}
