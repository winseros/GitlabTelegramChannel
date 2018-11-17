using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Xunit.Abstractions;

namespace TGramTestUtil
{
    internal class XUnitSink : ILogEventSink
    {
        private static readonly MessageTemplateTextFormatter formatter = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff}|{Level:u3}|{SourceContext}|{Message:lj}", null);
        private static readonly MessageTemplateTextFormatter exceptionFormatter = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff}|{Level:u3}|{SourceContext}|{Message:lj}|{NewLine}{Exception}", null);

        private readonly ITestOutputHelper outputHelper;

        public XUnitSink(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public void Emit(LogEvent logEvent)
        {
            var writer = new StringWriter { NewLine = "" };
            if (logEvent.Exception == null)
                XUnitSink.formatter.Format(logEvent, writer);
            else
                XUnitSink.exceptionFormatter.Format(logEvent, writer);

            this.outputHelper.WriteLine(writer.ToString());
        }
    }
}