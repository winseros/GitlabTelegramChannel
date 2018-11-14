using System;
using System.Runtime.Serialization;

namespace TGramCommon.Exceptions
{
    public class ConfigurationException: ApplicationException
    {
        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }
    }
}
