namespace TGramDaemon.Services.MessageHandler
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler CreateHandler();
    }
}