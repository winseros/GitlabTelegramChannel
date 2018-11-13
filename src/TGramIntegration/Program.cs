using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace TGramIntegration
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseSerilog((context, configuration) => Program.ConfigureSerilog(configuration, context))
                   .UseStartup<Startup>();

        private static void ConfigureSerilog(LoggerConfiguration configuration, WebHostBuilderContext context)
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        }
    }
}
