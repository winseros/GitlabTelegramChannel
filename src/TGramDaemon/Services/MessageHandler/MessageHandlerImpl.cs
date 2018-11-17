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
    public sealed class MessageHandlerImpl: IDisposable, IMessageHandler
    {
        private readonly ITelegramService telegramService;
        private readonly MessageHandlerOptions options;
        private readonly ILogger logger;
        private CancellationTokenSource cts;
        private PullSocket socket;
        private Task process;
        private bool started;

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
            if (this.started)
                throw new InvalidOperationException("The handler has already been started");

            this.started = true;
            this.logger.LogDebug("Starting the message handler");
            this.cts = new CancellationTokenSource();
            this.socket = new PullSocket();
            this.socket.Connect(this.options.Address);
            this.process = Task.Run(this.Process, this.cts.Token);
        }

        public Task StopAsync(CancellationToken ct)
        {
            if (!this.started)
                throw new InvalidOperationException("The handler has not been started");

            this.started = false;
            this.logger.LogDebug("Stopping the message handler");
            this.cts.Cancel();
            this.socket.Close();

            var cancel = new TaskCompletionSource<object>();
            ct.Register(() => cancel.SetCanceled());

            return Task.WhenAny(this.process, cancel.Task);
        }

        public void Dispose()
        {
            this.cts?.Dispose();
            this.socket?.Dispose();
        }

        private async Task Process()
        {
            this.logger.LogDebug("The message handler has started");
            while (!this.cts.IsCancellationRequested)
            {
                try
                {
                    this.cts.Token.ThrowIfCancellationRequested();
                    await this.ProcessCycle();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "An error occurred while running the NetMq message handler");
                }
            }
            this.logger.LogDebug("The message handler has stopped");
        }

        private async Task ProcessCycle()
        {
            string markDown = this.socket.ReceiveFrameString();
            this.logger.LogTrace("Received the message: {0}", markDown);
            await this.telegramService.SendMessageAsync(markDown, this.cts.Token);
        }
    }
}
