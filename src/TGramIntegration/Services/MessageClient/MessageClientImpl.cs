using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;

namespace TGramIntegration.Services.MessageClient
{
    public class MessageClientImpl : IMessageClient, IDisposable
    {
        private readonly ILogger logger;
        private readonly MessageClientOptions options;
        private PushSocket socket;

        public MessageClientImpl(ILogger<MessageClientImpl> logger, IOptions<MessageClientOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
            this.options.ThrowIfInvalid();
        }

        public void ScheduleDelivery(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            this.logger.LogDebug("Sending the message {0} to socket {1}", message, this.options.Address);

            this.OpenSocketSafe();
            this.socket.SendFrame(message);
        }

        private void OpenSocketSafe()
        {
            if (this.socket == null)
            {
                lock (this.options)
                {
                    if (this.socket == null)
                    {
                        this.logger.LogDebug("Trying to open a socket at address {0}", this.options.Address);
                        this.socket = new PushSocket(this.options.Address);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.logger.LogDebug("Disposing the socket");
            this.socket?.Dispose();
        }
    }
}