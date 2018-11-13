using Microsoft.AspNetCore.Authentication;
using TGramIntegration.Exceptions;

namespace TGramIntegration.Authentication
{
    public class GitlabAuthenticationOptions: AuthenticationSchemeOptions
    {
        public string Token { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Token))
                throw new ConfigurationException($"The {nameof(GitlabAuthenticationOptions)}.{nameof(GitlabAuthenticationOptions.Token)} is not configured");
        }
    }
}
