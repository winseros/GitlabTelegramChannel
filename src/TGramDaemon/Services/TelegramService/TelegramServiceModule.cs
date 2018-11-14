using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGramDaemon.Services.TelegramService
{
    internal static class TelegramServiceModule
    {
        public static void AddTelegramService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelegramOptions>(configuration.GetSection("TGram"));
            services.AddSingleton<ITelegramService, TelegramServiceImpl>();
            services.AddTelegramClient();
        }
    }
}
