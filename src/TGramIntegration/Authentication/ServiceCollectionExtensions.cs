using System;
using Microsoft.AspNetCore.Authentication;

namespace TGramIntegration.Authentication
{
    internal static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddGitlab(this AuthenticationBuilder builder, Action<GitlabAuthenticationOptions> configure)
        {
            return builder.AddScheme<GitlabAuthenticationOptions, GitlabAuthenticationHandler>(GitlabAuthenticationDefaults.Scheme, GitlabAuthenticationDefaults.Scheme, configure);
        }
    }
}
