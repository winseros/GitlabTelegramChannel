using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TGramCommon;

namespace TGramWeb.Integration
{
    public static class IntegrationUtils
    {
        public static Task StartApplication(short port, CancellationToken ct)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Gitlab}:Token", "a-gitlab-token", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Token", "a-telegram-token", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Channel", "a-telegram-channel", EnvironmentVariableTarget.Process);

            return Task.Run(() => Program.RunApplication(new string[0], ct), ct);
        }

        public static Task StartListener<TMiddleware>(CancellationToken ct) where TMiddleware: IMiddleware
        {
            return Task.Run(() =>
            {
                IWebHost host = new WebHostBuilder()
                                .CaptureStartupErrors(true)
                                .UseKestrel(options => { options.Listen(IPAddress.Loopback, 9549); })
                                .UseStartup<Startup<TMiddleware>>()
                                .Build();

                using (host)
                {
                    host.StartAsync(ct).GetAwaiter().GetResult();
                }
            }, ct);
        }

        public class Startup<TMiddleware>
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<TMiddleware>();
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(Uri uri, ushort timeoutSeconds = 20)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                HttpResponseMessage message = await client.GetAsync(uri);
                return message;
            }
        }
    }
}
