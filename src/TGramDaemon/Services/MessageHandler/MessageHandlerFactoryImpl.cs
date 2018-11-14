using System;
using Microsoft.Extensions.DependencyInjection;

namespace TGramDaemon.Services.MessageHandler
{
    public class MessageHandlerFactoryImpl : IMessageHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public MessageHandlerFactoryImpl(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IMessageHandler CreateHandler()
        {
            var service = this.serviceProvider.GetService<IMessageHandler>();
            return service;
        }
    }
}
