using Microsoft.AspNetCore.Authentication;
using TGramCommon;
using TGramCommon.Exceptions;

namespace TGramWeb.Authentication
{
    public class GitlabAuthenticationOptions: AuthenticationSchemeOptions
    {
        public string Token { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Token))
                throw new ConfigurationException($"The \"{ConfigKeys.Gitlab}:{nameof(GitlabAuthenticationOptions.Token)}\" is not configured");
        }
    }
}
