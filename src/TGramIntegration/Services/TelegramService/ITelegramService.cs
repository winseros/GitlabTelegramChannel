using System.Threading;
using System.Threading.Tasks;

namespace TGramIntegration.Services.TelegramService
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string message, CancellationToken ct);
    }
}
