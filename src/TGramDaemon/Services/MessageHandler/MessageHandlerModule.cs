using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGramDaemon.Services.MessageHandler
{
    internal static class MessageHandlerModule
    {
        public static void AddMessageHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessageHandlerFactory, MessageHandlerFactoryImpl>();
            services.AddTransient<IMessageHandler, MessageHandlerImpl>();
            services.Configure<MessageHandlerOptions>(MessageHandlerOptions.From(configuration));
        }
    }
}
