using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using TGramCommon;

namespace TGramWeb.Integration
{
    internal static class IntegrationUtils
    {
        private const string gitlabToken = "a-gitlab-token";

        public static Task StartApplicationAsync(short port, CancellationToken ct)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Gitlab}:Token", IntegrationUtils.gitlabToken, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Token", "a-telegram-token", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Channel", "a-telegram-channel", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Endpoint", "http://localhost:47995", EnvironmentVariableTarget.Process);

            return Task.Run(() => Program.RunApplication(new string[0], ct), ct);
        }

        public static IWebHost CreateListener()
        {
            IWebHost host = new WebHostBuilder()
                            .CaptureStartupErrors(true)
                            .UseKestrel(options => { options.ListenLocalhost(47995); })
                            .UseStartup<Startup>()
                            .Build();
            host.ServerFeatures.Set(new RequestGate());
            return host;
        }

        public static Task StartListenerAsync(this IWebHost host, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                using (host)
                {
                    host.StartAsync(ct);
                    host.WaitForShutdown();
                }
            }, ct);
        }

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseDeveloperExceptionPage();
                app.Run(context =>
                {
                    var server = context.RequestServices.GetRequiredService<IServer>();
                    var gate = server.Features.Get<RequestGate>();
                    gate.Request = context.Request;
                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                    return Task.CompletedTask;
                });
            }
        }

        public static async Task<HttpRequest> GetCapturedRequestAsync(this IWebHost host)
        {
            var gate = host.ServerFeatures.Get<RequestGate>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (!cts.IsCancellationRequested && gate.Request == null)
            {
                await Task.Yield();
            }

            HttpRequest result;
            if (gate.Request == null)
            {
                throw new Exception("Daemon sent no requests to telegram during the time interval");
            }
            else
            {
                result = gate.Request;
                gate.Request = null;
            }

            return result;
        }

        public class RequestGate
        {
            private volatile HttpRequest request;

            public HttpRequest Request
            {
                get => this.request;
                set => this.request = value;
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

        public static async Task<HttpResponseMessage> HttpPostAsync<TData>(Uri uri, TData data, ushort timeoutSeconds = 20)
        {
            var formatter = new JsonMediaTypeFormatter {SerializerSettings = {ContractResolver = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()}}};
            using (var content = new ObjectContent<TData>(data, formatter))
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                client.DefaultRequestHeaders.Add("X-Gitlab-Token", IntegrationUtils.gitlabToken);
                HttpResponseMessage message = await client.PostAsync(uri, content);
                return message;
            }
        }
    }
}
