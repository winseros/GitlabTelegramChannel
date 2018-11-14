using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGramDaemon.Services.Daemon
{
    internal static class DaemonModule
    {
        public static void AddDaemon(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<Daemon>();
            services.Configure<DaemonOptions>(configuration.GetSection("Daemon"));
        }
    }
}
