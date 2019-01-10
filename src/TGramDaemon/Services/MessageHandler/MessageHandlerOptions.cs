using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using TGramCommon;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.MessageHandler
{
    [DebuggerDisplay("Address: {" + nameof(MessageHandlerOptions.Address) + "}")]
    public class MessageHandlerOptions
    {
        internal static IConfiguration From(IConfiguration config) => config.GetSection(ConfigKeys.Daemon);

        public string Address { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Address))
                throw new ConfigurationException($"The \"{ConfigKeys.Daemon}:{nameof(MessageHandlerOptions.Address)}\" setting is not configured");
        }
    }
}
