using System;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TGramIntegration.Authentication
{
    public class GitlabAuthenticationHandler: AuthenticationHandler<GitlabAuthenticationOptions>
    {
        private static readonly AuthenticationTicket authTicket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, "GitLab")}, GitlabAuthenticationDefaults.Scheme)), GitlabAuthenticationDefaults.Scheme);

        public GitlabAuthenticationHandler(IOptionsMonitor<GitlabAuthenticationOptions> options,
                                           ILoggerFactory logger,
                                           UrlEncoder encoder,
                                           ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task InitializeHandlerAsync()
        {
            this.Options.ThrowIfInvalid();
            return base.InitializeHandlerAsync();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result;
            StringValues token = this.Request.Headers["X-Gitlab-Token"];
            if (token.Count > 0)
            {
                result = string.Equals(token[0], this.Options.Token, StringComparison.Ordinal) 
                    ? AuthenticateResult.Success(GitlabAuthenticationHandler.authTicket) 
                    : AuthenticateResult.Fail("The provided gitlab token didn't match the expected one");
            }
            else
            {
                result = AuthenticateResult.Fail("The request didn't contain the \"X-Gitlab-Token\" header");
            }

            return Task.FromResult(result);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            AuthenticateResult authResult = await this.HandleAuthenticateOnceSafeAsync();
            if (!authResult.Succeeded)
            {
                this.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }
}
