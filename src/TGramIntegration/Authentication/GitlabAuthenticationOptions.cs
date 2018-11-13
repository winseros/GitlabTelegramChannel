using Microsoft.AspNetCore.Authentication;

namespace TGramIntegration.Authentication
{
    public class GitlabAuthenticationOptions: AuthenticationSchemeOptions
    {
        public string Token { get; set; }
    }
}
