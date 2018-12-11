using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using TGramCommon;

namespace TGramWeb.Integration
{
    internal static class IntegrationUtils
    {
        private const string gitlabToken = "a-gitlab-token";
        private const string telegramToken = "a-telegram-token";
        private const string telegramChannel = "a-telegram-channel";
        private const string telegramEndpoint = "http://localhost:47995";

        public static async Task StartApplicationAsync(short port, CancellationToken ct)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://::{port}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Gitlab}:Token", IntegrationUtils.gitlabToken, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Token", IntegrationUtils.telegramToken, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Channel", IntegrationUtils.telegramChannel, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable($"{ConfigKeys.Telegram}:Endpoint", IntegrationUtils.telegramEndpoint, EnvironmentVariableTarget.Process);

            await IntegrationUtils.WaitForThePortReleased(port);
            await Task.Run(() => Program.RunApplication(new string[0], ct), ct);
        }

        public static IWebHost CreateListener()
        {
            IWebHost host = new WebHostBuilder()
                            .CaptureStartupErrors(true)
                            .UseKestrel(options => { options.ListenLocalhost(new Uri(IntegrationUtils.telegramEndpoint).Port); })
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
                    host.WaitForShutdownAsync(ct).GetAwaiter().GetResult();
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

                    var feature = context.Features.Get<IHttpRequestFeature>();
                    var mockRequest = new HttpRequestFeature
                    {
                        Scheme = feature.Scheme,
                        RawTarget = feature.RawTarget,
                        Protocol = feature.Protocol,
                        Method = feature.Method,
                        Path = feature.Path,
                        PathBase = feature.PathBase,
                        QueryString = feature.QueryString,
                        Headers = feature.Headers,
                        Body = new MemoryStream()
                    };
                    feature.Body.CopyTo(mockRequest.Body, 1024);
                    mockRequest.Body.Seek(0, SeekOrigin.Begin);

                    gate.Request = mockRequest;

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

            if (gate.Request == null)
                throw new Exception("Daemon sent no requests to telegram during the time interval");

            var features = new FeatureCollection();
            features.Set(gate.Request);
            gate.Request = null;

            return new DefaultHttpRequest(new DefaultHttpContext(features));
        }

        public class RequestGate
        {
            private volatile IHttpRequestFeature request;

            public IHttpRequestFeature Request
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

        private static async Task WaitForThePortReleased(short port)
        {
            bool busy;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            do
            {
                busy = false;
                IPGlobalProperties globalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listeners = globalProperties.GetActiveTcpListeners();
                foreach (IPEndPoint listener in listeners)
                {
                    if (listener.Port == port)
                    {
                        if (cts.IsCancellationRequested)
                            throw new Exception($"The timeout of waiting for the {port} port gets released has expired");

                        busy = true;
                        await Task.Yield();
                        break;
                    }
                }
            } while (busy);
        }
    }
}
