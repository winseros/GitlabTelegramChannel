using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGramIntegration.Services.GitlabProcessService;
using TGramIntegration.Services.TelegramService;

namespace TGramIntegration.Services
{
    internal static class ServicesModule
    {
        internal static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGitlabProcessService();
            services.AddTelegramService(configuration);
        }
    }
}
