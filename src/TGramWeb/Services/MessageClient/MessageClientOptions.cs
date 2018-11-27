using System.Diagnostics;
using TGramCommon.Exceptions;

namespace TGramWeb.Services.MessageClient
{
    [DebuggerDisplay("Address: {Address}")]
    public class MessageClientOptions
    {
        public string Address { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Address))
                throw new ConfigurationException($"The {nameof(MessageClientOptions)}.{nameof(MessageClientOptions.Address)} setting is not configured");
        }
    }
}
