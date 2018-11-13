using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGramIntegration.Services.MessageClient
{
    internal static class MessageClientModule
    {
        public static void AddMessageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessageClient, MessageClientImpl>();
            services.Configure<MessageClientOptions>(configuration.GetSection("Daemon"));
        }
    }
}
