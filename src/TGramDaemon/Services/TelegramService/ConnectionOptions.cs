using System;
using System.Diagnostics;
using System.Text;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.TelegramService
{
    [DebuggerDisplay("Attempts: {Attempts}, Interval: {Interval}")]
    public class ConnectionOptions
    {
        public byte Timeout { get; set; } = 20;

        public byte Attempts { get; set; } = 2;

        public byte Interval { get; set; } = 10;

        internal void ThrowIfInvalid()
        {
            StringBuilder sb = null;
            byte counter = 1;

            if (this.Timeout <= 0)
            {
                sb = new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The {nameof(ConnectionOptions)}.{nameof(this.Timeout)} must be a positive number");
            }

            if (this.Attempts <= 0)
            {
                sb = sb ?? new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The {nameof(ConnectionOptions)}.{nameof(this.Attempts)} must be a positive number");
            }

            if (this.Interval <= 0)
            {
                sb = sb ?? new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The {nameof(ConnectionOptions)}.{nameof(this.Interval)} must be a positive number");
            }

            if (sb != null)
                throw new ConfigurationException(sb.ToString());
        }
    }
}