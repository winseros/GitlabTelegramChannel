using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGramWeb.Services.MessageClient
{
    internal static class MessageClientModule
    {
        public static void AddMessageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessageClient, MessageClientImpl>();
            services.Configure<MessageClientOptions>(MessageClientOptions.From(configuration));
        }
    }
}
