using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TGramWeb.Services.GitlabProcessService
{
    public class GitlabProcessServiceImpl : IGitlabProcessService
    {
        private readonly IEnumerable<IGitlabProcessor> requestProcessors;
        private readonly ILogger logger;

        public GitlabProcessServiceImpl(IEnumerable<IGitlabProcessor> requestProcessors,
                                        ILogger<GitlabProcessServiceImpl> logger)
        {
            this.requestProcessors = requestProcessors;
            this.logger = logger;
        }

        public RequestProcessResult ProcessRequest(JObject request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (this.logger.IsEnabled(LogLevel.Trace))
                this.logger.LogTrace(request.ToString());

            RequestProcessResult result = RequestProcessResult.CreateNoResult();

            foreach (IGitlabProcessor processor in this.requestProcessors)
            {
                if (this.logger.IsEnabled(LogLevel.Debug))
                    this.logger.LogDebug("Calling the processor: {0}", processor.GetType().FullName);

                result = processor.Process(request);

                this.logger.LogDebug("The processor call result was: {0}", result);

                if (!result.NoResult)
                {
                    this.logger.LogDebug("A non-empty result was received - stop further processing");
                    break;
                }
            }

            if (result.NoResult)
            {
                this.logger.LogDebug("No particular processing result was obtained - exit with NoResult status");
                result = RequestProcessResult.CreateFailure("The system is not capable of processing such requests");
            }

            return result;
        }
    }
}