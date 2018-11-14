using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TGramDaemon.Services.MessageHandler;

namespace TGramDaemon.Services.Daemon
{
    public class Daemon : IHostedService
    {
        private readonly IMessageHandlerFactory messageHandlerFactory;
        private readonly DaemonOptions options;
        private readonly ILogger logger;
        private IMessageHandler[] messageHandlers;

        public Daemon(IMessageHandlerFactory messageHandlerFactory,
                      IOptions<DaemonOptions> options,
                      ILogger<Daemon> logger)
        {
            this.messageHandlerFactory = messageHandlerFactory;
            this.options = options.Value;
            this.options.ThrowIfInvalid();
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken ct)
        {
            this.logger.LogDebug("Starting the daemon");

            this.messageHandlers = new IMessageHandler[this.options.ThreadCount];
            for (byte i = 0; i < this.options.ThreadCount; i++)
            {
                IMessageHandler handler = this.messageHandlerFactory.CreateHandler();
                handler.Start();
                this.messageHandlers[i] = handler;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            this.logger.LogDebug("Stopping the handlers");

            return Task.WhenAll(this.messageHandlers.Select(p => p.StopAsync(ct)));
        }
    }
}