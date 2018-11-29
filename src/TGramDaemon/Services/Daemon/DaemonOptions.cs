using Microsoft.Extensions.Configuration;
using TGramCommon;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.Daemon
{
    public class DaemonOptions
    {
        internal static IConfiguration From(IConfiguration config) => config.GetSection(ConfigKeys.Daemon);

        public short ThreadCount { get; set; }

        public void ThrowIfInvalid()
        {
            if (this.ThreadCount <= 0)
                throw new ConfigurationException($"The \"{ConfigKeys.Daemon}:{nameof(DaemonOptions.ThreadCount)}\" must be a positive number");
        }
    }
}
