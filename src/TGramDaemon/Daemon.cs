using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TGramDaemon
{
    public class Daemon: IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }
    }
}
