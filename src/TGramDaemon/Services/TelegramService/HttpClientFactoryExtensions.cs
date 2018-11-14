using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TGramDaemon.Services.TelegramService
{
    public static class HttpClientFactoryExtensions
    {
        private const string clientName = "TelegramHttpClient";

        public static IHttpClientBuilder AddTelegramClient(this IServiceCollection services, Action<HttpClient> configureClient = null)
        {
            return configureClient == null
                ? services.AddHttpClient(HttpClientFactoryExtensions.clientName)
                : services.AddHttpClient(HttpClientFactoryExtensions.clientName, configureClient);
        }

        public static HttpClient CreateTelegramClient(this IHttpClientFactory factory)
        {
            HttpClient client = factory.CreateClient(HttpClientFactoryExtensions.clientName);
            return client;
        }
    }
}
