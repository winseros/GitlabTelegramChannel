using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.MessageClient;
using TGramWeb.Services.TelegramService;

namespace TGramWeb.Services
{
    internal static class ServicesModule
    {
        internal static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGitlabProcessService();
            services.AddMessageClient(configuration);
            services.AddTelegramService(configuration);
        }
    }
}
