using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGramWeb.Authentication;
using TGramWeb.Services;

namespace TGramWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
                    {
                        options.DefaultChallengeScheme = GitlabAuthenticationDefaults.Scheme;
                        options.DefaultScheme = GitlabAuthenticationDefaults.Scheme;
                    })
                    .AddJwtBearer()
                    .AddGitlab(options => this.Configuration.InflateGitlabOptions(options));


            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                                        .AddAuthenticationSchemes(GitlabAuthenticationDefaults.Scheme)
                                        .RequireAuthenticatedUser()
                                        .Build();
            });

            services.AddApplicationServices(this.Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lt)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
