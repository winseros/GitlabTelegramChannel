using System.Diagnostics;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.MessageHandler
{
    [DebuggerDisplay("Address: {Address}")]
    public class MessageHandlerOptions
    {
        public string Address { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Address))
                throw new ConfigurationException($"The {nameof(MessageHandlerOptions)}.{nameof(MessageHandlerOptions.Address)} setting is not configured");
        }
    }
}
