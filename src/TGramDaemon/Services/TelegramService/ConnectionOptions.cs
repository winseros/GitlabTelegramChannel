using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using TGramCommon;
using TGramCommon.Exceptions;

namespace TGramDaemon.Services.TelegramService
{
    [DebuggerDisplay("Attempts: {Attempts}, Interval: {Interval}")]
    public class ConnectionOptions
    {
        internal static ConnectionOptions FromConfiguration(IConfiguration config)
        {
            ConnectionOptions options = config.GetSection(ConfigKeys.TelegramConnection).Get<ConnectionOptions>()
                                        ?? new ConnectionOptions();
            return options;
        }

        public short Timeout { get; set; } = 20;

        public short Attempts { get; set; } = 2;

        public short Interval { get; set; } = 10;

        internal void ThrowIfInvalid()
        {
            StringBuilder sb = null;
            byte counter = 1;

            if (this.Timeout <= 0)
            {
                sb = new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The \"{ConfigKeys.TelegramConnection}:{nameof(this.Timeout)}\" must be a positive number");
                counter++;
            }

            if (this.Attempts <= 0)
            {
                sb = sb ?? new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The \"{ConfigKeys.TelegramConnection}:{nameof(this.Attempts)}\" must be a positive number");
                counter++;
            }

            if (this.Interval <= 0)
            {
                sb = sb ?? new StringBuilder();
                if (counter > 1) sb.Append(Environment.NewLine);
                sb.Append($"{counter}. The \"{ConfigKeys.TelegramConnection}:{nameof(this.Interval)}\" must be a positive number");
            }

            if (sb != null)
                throw new ConfigurationException(sb.ToString());
        }
    }
}