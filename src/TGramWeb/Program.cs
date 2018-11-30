using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using TGramCommon.Exceptions;
using TGramDaemon;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace TGramWeb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Program.RunApplication(args);
            }
            catch (ConfigurationException ex)
            {
                Console.WriteLine("The application didn't start - the app configuration has the following issues:");
                Console.WriteLine(ex.Message);
            }
        }

        internal static void RunApplication(string[] args, CancellationToken ct = default(CancellationToken))
        {
            using (IHost daemonHost = DaemonHost.CreateBuilder(args).Build())
            using (IWebHost webHost = Program.CreateWebHostBuilder(args).Build())
            {
                var webLifetime = (IApplicationLifetime) webHost.Services.GetService(typeof(IApplicationLifetime));
                webLifetime.ApplicationStopping.Register(() => daemonHost.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult());

                daemonHost.Start();
                webHost.RunAsync(ct.CanBeCanceled ? ct : new CancellationToken()).GetAwaiter().GetResult();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseSerilog(Program.ConfigureSerilog)
                   .UseStartup<Startup>();

        private static void ConfigureSerilog(WebHostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        }
    }
}
