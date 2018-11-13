namespace TGramWeb.Services.TelegramService
{
    public class SendMessageRequest
    {
        public string ChatId { get; set; }

        public string Text { get; set; }

        public string ParseMode { get; set; }
    }
}
