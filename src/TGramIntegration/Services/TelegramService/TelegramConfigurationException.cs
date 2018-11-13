using System.Runtime.Serialization;
using TGramIntegration.Exceptions;

namespace TGramIntegration.Services.TelegramService
{
    public class TelegramConfigurationException : ConfigurationException
    {
        protected TelegramConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TelegramConfigurationException(string message)
            : base(message)
        {
        }
    }
}
