using System.Runtime.Serialization;
using TGramIntegration.Exceptions;

namespace TGramIntegration.Authentication
{
    public class GitlabConfigurationException: ConfigurationException
    {
        protected GitlabConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GitlabConfigurationException(string message)
            : base(message)
        {
        }
    }
}
