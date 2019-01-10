using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using TGramCommon;
using TGramCommon.Exceptions;

namespace TGramWeb.Services.MessageClient
{
    [DebuggerDisplay("Address: {" + nameof(MessageClientOptions.Address) + "}")]
    public class MessageClientOptions
    {
        internal static IConfiguration From(IConfiguration config) => config.GetSection(ConfigKeys.Daemon);

        public string Address { get; set; }

        internal void ThrowIfInvalid()
        {
            if (string.IsNullOrEmpty(this.Address))
                throw new ConfigurationException($"The \"{ConfigKeys.Daemon}:{nameof(MessageClientOptions.Address)}\" setting is not configured");
        }
    }
}
