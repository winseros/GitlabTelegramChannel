using System.Threading;
using System.Threading.Tasks;

namespace TGramDaemon.Services.MessageHandler
{
    public interface IMessageHandler
    {
        void Start();

        Task StopAsync(CancellationToken ct);
    }
}