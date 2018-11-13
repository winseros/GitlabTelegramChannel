using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using TGramDaemon;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace TGramWeb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost daemonHost = DaemonHost.CreateBuilder(args).Build();
            IWebHost webHost = Program.CreateWebHostBuilder(args).Build();

            var webLifetime = (IApplicationLifetime) webHost.Services.GetService(typeof(IApplicationLifetime));
            webLifetime.ApplicationStarted.Register(() => daemonHost.Start());
            webLifetime.ApplicationStopping.Register(() => daemonHost.StopAsync().Wait());

            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseSerilog(Program.ConfigureSerilog)
                   .UseStartup<Startup>();

        private static void ConfigureSerilog(WebHostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        }
    }
}
