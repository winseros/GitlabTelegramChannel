namespace TGramIntegration.Services.MessageClient
{
    public interface IMessageClient
    {
        void ScheduleDelivery(string message);
    }
}
