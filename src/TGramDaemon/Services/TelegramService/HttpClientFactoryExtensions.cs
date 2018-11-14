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
        private const string clientName = "TelegramHttpClient";

        public static void AddTelegramClient(this IServiceCollection services, IConfiguration configuration)
        {
            RetryOptions retry = configuration.GetSection("TGram:Retry").Get<RetryOptions>() ?? new RetryOptions();
            retry.ThrowIfInvalid();

            services.AddHttpClient(HttpClientFactoryExtensions.clientName)
                    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(retry.Attempts, i => TimeSpan.FromSeconds(retry.Interval)))
                    .ConfigureHttpClient((di, client) =>
                    {
                        TelegramOptions options = di.GetRequiredService<IOptions<TelegramOptions>>().Value;
                        client.Timeout = TimeSpan.FromSeconds(options.Timeout);
                        client.BaseAddress = options.Endpoint;
                    });
        }

        public static HttpClient CreateTelegramClient(this IHttpClientFactory factory)
        {
            HttpClient client = factory.CreateClient(HttpClientFactoryExtensions.clientName);
            return client;
        }
    }
}