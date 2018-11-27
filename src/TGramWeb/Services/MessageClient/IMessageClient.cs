namespace TGramWeb.Services.MessageClient
{
    public interface IMessageClient
    {
        void ScheduleDelivery(string message);
    }
}
