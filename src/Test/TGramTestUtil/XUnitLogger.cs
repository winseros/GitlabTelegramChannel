using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Parsing;

namespace TGramTestUtil
{
    internal class XUnitLogger : ILogger
    {
        private static readonly MessageTemplateParser messageTemplateParser = new MessageTemplateParser();
        private readonly Serilog.ILogger logger;

        public XUnitLogger(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEventLevel level = XUnitLogger.ConvertLevel(logLevel);
            if (!this.logger.IsEnabled(level))
                return;

            string str = null;
            var logEventPropertyList = new List<LogEventProperty>();
            if (state as object is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
            {
                foreach (KeyValuePair<string, object> keyValuePair in keyValuePairs)
                {
                    if (keyValuePair.Key == "{OriginalFormat}" && keyValuePair.Value is string s)
                    {
                        str = s;
                    }
                    else if (keyValuePair.Key.StartsWith("@"))
                    {
                        if (this.logger.BindProperty(keyValuePair.Key.Substring(1), keyValuePair.Value, true, out LogEventProperty property))
                            logEventPropertyList.Add(property);
                    }
                    else
                    {
                        if (this.logger.BindProperty(keyValuePair.Key, keyValuePair.Value, false, out LogEventProperty property))
                            logEventPropertyList.Add(property);
                    }
                }

                Type type = state.GetType();
                TypeInfo typeInfo = type.GetTypeInfo();
                if (str == null && !typeInfo.IsGenericType)
                {
                    str = "{" + type.Name + ":l}";
                    if (this.logger.BindProperty(type.Name, XUnitLogger.AsLoggableValue(state, formatter), false, out LogEventProperty property))
                        logEventPropertyList.Add(property);
                }
            }

            if (str == null)
            {
                var propertyName = (string) null;
                if (state != null)
                {
                    propertyName = "State";
                    str = "{State:l}";
                }
                else if (formatter != null)
                {
                    propertyName = "Message";
                    str = "{Message:l}";
                }

                if (propertyName != null && this.logger.BindProperty(propertyName, XUnitLogger.AsLoggableValue(state, formatter), false, out LogEventProperty property))
                    logEventPropertyList.Add(property);
            }

            if (eventId.Id != 0 || eventId.Name != null)
                logEventPropertyList.Add(XUnitLogger.CreateEventIdProperty(eventId));

            MessageTemplate messageTemplate = XUnitLogger.messageTemplateParser.Parse(str ?? "");
            var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, messageTemplate, logEventPropertyList);
            this.logger.Write(logEvent);
        }

        private static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
        {
            var obj = (object) state;
            if (formatter != null)
                obj = formatter(state, null);
            return obj;
        }

        private static LogEventLevel ConvertLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Verbose;
            }
        }

        private static LogEventProperty CreateEventIdProperty(EventId eventId)
        {
            var logEventPropertyList = new List<LogEventProperty>(2);
            if (eventId.Id != 0)
                logEventPropertyList.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
            if (eventId.Name != null)
                logEventPropertyList.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
            return new LogEventProperty("EventId", new StructureValue(logEventPropertyList));
        }
    }

    internal class XUnitLogger<T> : XUnitLogger, ILogger<T>
    {
        public XUnitLogger(Serilog.ILogger logger) : base(logger)
        {
        }
    }
}