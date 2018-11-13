using Newtonsoft.Json.Linq;

namespace TGramIntegration.Services.GitlabProcessService
{
    public interface IGitlabProcessor
    {
        RequestProcessResult Process(JObject request);
    }
}
