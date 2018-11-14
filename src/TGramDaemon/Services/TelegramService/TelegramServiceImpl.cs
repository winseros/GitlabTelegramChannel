using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TGramDaemon.Services.TelegramService
{
    public class TelegramServiceImpl : ITelegramService
    {
        private readonly MediaTypeFormatter snakeCaseFormatter = new JsonMediaTypeFormatter {SerializerSettings = new JsonSerializerSettings {ContractResolver = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()}}};
        private readonly TelegramOptions options;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger logger;
        private readonly string sendMessageEndpoint;

        public TelegramServiceImpl(IOptions<TelegramOptions> options,
                                   IHttpClientFactory clientFactory,
                                   ILogger<TelegramServiceImpl> logger)
        {
            this.clientFactory = clientFactory;
            this.logger = logger;
            this.options = options.Value;
            this.options.ThrowIfInvalid();

            this.sendMessageEndpoint = string.Concat("/bot", this.options.Token, "/sendMessage");
        }

        public Task SendMessageAsync(string message, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            return this.SendMessageImplAsync(message, ct);
        }

        private async Task SendMessageImplAsync(string message, CancellationToken ct)
        {
            var request = new SendMessageRequest
            {
                ChatId = this.options.Channel,
                ParseMode = "Markdown",
                Text = message
            };

            this.logger.LogDebug("Sending the request: {0} to the endpoint", request);

            using (HttpClient client = this.clientFactory.CreateTelegramClient())
            {
                this.logger.LogDebug("Using endpoint: {0}", client.BaseAddress);

                var content = new ObjectContent(typeof(SendMessageRequest), request, this.snakeCaseFormatter);
                HttpResponseMessage response = await client.PostAsync(this.sendMessageEndpoint, content, ct);

                this.logger.LogDebug("Received the response with status code: {0}", response.StatusCode);

                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    string text = await response.Content.ReadAsStringAsync();
                    this.logger.LogTrace("Received the response: {0}", text);
                }
            }
        }
    }
}