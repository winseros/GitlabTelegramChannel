using System;
using System.Diagnostics;

namespace TGramWeb.Services.GitlabProcessService
{
    [DebuggerDisplay("Success: {Success}, NoResult: {NoResult}, Reason: {Reason}")]
    public struct RequestProcessResult
    {
        public static RequestProcessResult CreateSuccess()
        {
            return new RequestProcessResult(true, null);
        }

        public static RequestProcessResult CreateFailure(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                throw new ArgumentNullException(nameof(reason));
            return new RequestProcessResult(false, reason);
        }

        public static RequestProcessResult CreateNoResult()
        {
            return new RequestProcessResult(false, true);
        }

        private RequestProcessResult(bool success, string reason)
        {
            this.NoResult = false;
            this.Success = success;
            this.Reason = reason;
        }

        private RequestProcessResult(bool success, bool noResult)
        {
            this.Success = success;
            this.NoResult = noResult;
            this.Reason = null;
        }

        public bool Success { get; }

        public bool NoResult { get; }

        public string Reason { get; }
    }
}
