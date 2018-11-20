using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService
{
    public interface IGitlabFormatter
    {
        RequestProcessResult TryFormat(JObject request, out string message);
    }
}
