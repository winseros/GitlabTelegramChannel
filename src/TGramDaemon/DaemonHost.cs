using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;

namespace TGramDaemon
{
    public static class DaemonHost
    {
        public static IHostBuilder CreateBuilder(string[] args)
        {
            return new HostBuilder()
                   .ConfigureAppConfiguration((context, builder) =>
                   {
                       builder.AddJsonFile("appsettings.json");
                       builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
                       builder.AddEnvironmentVariables();
                       builder.AddCommandLine(args);
                   }).ConfigureLogging(ConfigureSerilog)
                   .ConfigureServices(services => { services.AddHostedService<Daemon>(); });
        }

        private static void ConfigureSerilog(HostBuilderContext context, ILoggingBuilder configuration)
        {
            var conf = new LoggerConfiguration();
            conf.ReadFrom.Configuration(context.Configuration);
            configuration.Services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(conf.CreateLogger()));
        }
    }
}
