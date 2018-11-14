using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGramDaemon.Services.Daemon;
using TGramDaemon.Services.MessageHandler;
using TGramDaemon.Services.TelegramService;

namespace TGramDaemon.Services
{
    internal static class ServicesModule
    {
        internal static void AddDaemonServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDaemon(configuration);
            services.AddMessageHandler(configuration);
            services.AddTelegramService(configuration);
        }
    }
}
