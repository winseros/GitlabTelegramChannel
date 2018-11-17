using System;
using System.Diagnostics;
using System.Text;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.TelegramService
{
    [DebuggerDisplay("Channel: {Channel}, Endpoint: {Endpoint}, Token: {Token}")]
    public class TelegramOptions
    {
        public string Token { get; set; }

        public string Channel { get; set; }

        public Uri Endpoint { get; set; }

        internal void ThrowIfInvalid()
        {
            StringBuilder sb = null;
            byte counter = 1;
            if (string.IsNullOrEmpty(this.Token))
            {
                sb = new StringBuilder();
                sb.Append($"{counter}. The {nameof(TelegramOptions)}.{nameof(this.Token)} setting not configured");
            }

            if (string.IsNullOrEmpty(this.Channel))
            {
                sb = sb ?? new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The {nameof(TelegramOptions)}.{nameof(this.Channel)} setting not configured");
            }

            if (sb != null)
                throw new ConfigurationException(sb.ToString());
        }
    }
}
