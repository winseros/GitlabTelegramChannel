using System.Threading;
using System.Threading.Tasks;

namespace TGramDaemon.Services.TelegramService
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string message, CancellationToken ct);
    }
}
