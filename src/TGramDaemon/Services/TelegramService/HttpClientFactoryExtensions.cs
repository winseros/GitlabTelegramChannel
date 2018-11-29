using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace TGramDaemon.Services.TelegramService
{
    public static class HttpClientFactoryExtensions
    {
        internal const string ClientName = "TelegramHttpClient";

        public static void AddTelegramClient(this IServiceCollection services, IConfiguration configuration)
        {
            ConnectionOptions connOptions = ConnectionOptions.FromConfiguration(configuration);
            connOptions.ThrowIfInvalid();

            services.AddHttpClient(HttpClientFactoryExtensions.ClientName)
                    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(connOptions.Attempts, i => TimeSpan.FromSeconds(connOptions.Interval)))
                    .ConfigureHttpClient((di, client) =>
                    {
                        TelegramOptions tgramOptions = di.GetRequiredService<IOptions<TelegramOptions>>().Value;
                        client.Timeout = TimeSpan.FromSeconds(connOptions.Timeout);
                        client.BaseAddress = tgramOptions.Endpoint;
                    });
        }

        public static HttpClient CreateTelegramClient(this IHttpClientFactory factory)
        {
            HttpClient client = factory.CreateClient(HttpClientFactoryExtensions.ClientName);
            return client;
        }
    }
}