using System.Diagnostics;

namespace TGramDaemon.Services.TelegramService
{
    [DebuggerDisplay("ChatId: {ChatId}, Text: {Text}")]
    public class SendMessageRequest
    {
        public string ChatId { get; set; }

        public string Text { get; set; }

        public string ParseMode { get; set; }
    }
}
