using Serilog.Configuration;
using Serilog.Events;

namespace TGramTestUtil
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static void XUnit(this LoggerSinkConfiguration conf, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            conf.Sink(new XUnitSink(OutputHelperExtensions.Output), restrictedToMinimumLevel);
        }
    }
}
