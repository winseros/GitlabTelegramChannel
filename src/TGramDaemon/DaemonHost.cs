using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;
using TGramDaemon.Services;

namespace TGramDaemon
{
    public static class DaemonHost
    {
        public static IHostBuilder CreateBuilder(string[] args)
        {
            IHostBuilder b = new HostBuilder()
                             .ConfigureHostConfiguration(builder =>
                             {
                                 builder.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                             })
                             .ConfigureAppConfiguration((context, builder) =>
                             {
                                 builder.SetBasePath(Directory.GetCurrentDirectory());
                                 builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                                 builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                                 builder.AddEnvironmentVariables();

                                 if (args != null)
                                     builder.AddCommandLine(args);
                             }).ConfigureLogging(DaemonHost.ConfigureSerilog)
                             .ConfigureServices((context, services) =>
                             {
                                 services.AddDaemonServices(context.Configuration);
                             });
            return b;
        }

        private static void ConfigureSerilog(HostBuilderContext context, ILoggingBuilder configuration)
        {
            var conf = new LoggerConfiguration();
            conf.ReadFrom.Configuration(context.Configuration);
            configuration.Services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(conf.CreateLogger()));
        }
    }
}