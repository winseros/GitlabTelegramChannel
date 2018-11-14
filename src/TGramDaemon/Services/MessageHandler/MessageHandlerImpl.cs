using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using TGramDaemon.Services.TelegramService;

namespace TGramDaemon.Services.MessageHandler
{
    public class MessageHandlerImpl: IDisposable, IMessageHandler
    {
        private readonly ITelegramService telegramService;
        private readonly MessageHandlerOptions options;
        private readonly ILogger logger;
        private CancellationTokenSource cts;
        private PullSocket socket;

        public MessageHandlerImpl(ITelegramService telegramService,
                                  IOptions<MessageHandlerOptions> options,
                                  ILogger<MessageHandlerImpl> logger)
        {
            this.telegramService = telegramService;
            this.options = options.Value;
            this.options.ThrowIfInvalid();
            this.logger = logger;
        }

        public void Start()
        {
            if (this.socket != null)
                throw new InvalidOperationException("The handler has already been started");

            this.logger.LogDebug("Starting the message handler");
            this.cts = new CancellationTokenSource();
            this.socket = new PullSocket();
            this.socket.Connect(this.options.Address);
            Task.Run(() => this.Process(), this.cts.Token);
        }

        public void Stop()
        {
            if (this.socket == null)
                throw new InvalidOperationException("The handler has not been started");

            this.logger.LogDebug("Stopping the message handler");
            this.cts.Cancel();
            this.socket.Close();
        }

        public void Dispose()
        {
            this.cts?.Dispose();
            this.socket?.Dispose();
        }

        private void Process()
        {
            this.logger.LogDebug("The message handler has started");
            while (!this.cts.IsCancellationRequested)
            {
                try
                {
                    this.cts.Token.ThrowIfCancellationRequested();
                    this.ProcessCycle();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "An error occurred while running the NetMq message handler");
                }
            }
            this.logger.LogDebug("The message handler has stopped");
        }

        private void ProcessCycle()
        {
            string markDown = this.socket.ReceiveFrameString();
            this.logger.LogTrace("Received the message: {0}", markDown);
            this.telegramService.SendMessageAsync(markDown, this.cts.Token);
        }
    }
}
