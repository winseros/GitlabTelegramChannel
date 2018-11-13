using System.Threading;
using System.Threading.Tasks;

namespace TGramWeb.Services.TelegramService
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string message, CancellationToken ct);
    }
}
