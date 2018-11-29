using Microsoft.Extensions.Configuration;
using TGramCommon;

namespace TGramWeb.Authentication
{
    internal static class ConfigurationExtensions
    {
        public static void InflateGitlabOptions(this IConfiguration config, GitlabAuthenticationOptions options)
        {
            options.Token = config.GetValue<string>($"{ConfigKeys.Gitlab}:{nameof(GitlabAuthenticationOptions.Token)}");
        }
    }
}