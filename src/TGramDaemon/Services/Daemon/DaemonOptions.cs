using TGramCommon.Exceptions;

namespace TGramDaemon.Services.Daemon
{
    public class DaemonOptions
    {
        public byte ThreadCount { get; set; }

        public void ThrowIfInvalid()
        {
            if (this.ThreadCount <= 0)
                throw new ConfigurationException($"The {nameof(DaemonOptions)}.{nameof(DaemonOptions.ThreadCount)} must be a positive number in range from 1 to 255");
        }
    }
}
