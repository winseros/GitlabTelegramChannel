using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;
using Module = Autofac.Module;

namespace TGramTestUtil
{
    internal class XUnitModule : Module
    {
        private readonly ILogger logger;

        public XUnitModule(ITestOutputHelper output)
        {
            var config = new LoggerConfiguration();
            config.WriteTo.Sink(new XUnitSink(output), LogEventLevel.Verbose);
            config.MinimumLevel.Verbose();
            this.logger = config.CreateLogger();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += (sender, args) =>
            {
                bool Match(ParameterInfo info, IComponentContext context)
                {
                    return typeof(Microsoft.Extensions.Logging.ILogger).IsAssignableFrom(info.ParameterType);
                }

                object Provide(ParameterInfo info, IComponentContext context)
                {
                    Type loggerType = typeof(XUnitLogger<>).MakeGenericType(info.Member.DeclaringType);
                    ConstructorInfo constructor = loggerType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] {typeof(ILogger)}, null);
                    Debug.Assert(constructor != null);
                    object instance = constructor.Invoke(new object[] {this.logger.ForContext(info.Member.DeclaringType)});
                    return instance;
                }

                args.Parameters = args.Parameters.Union(new[] {new ResolvedParameter(Match, Provide)});
            };
        }
    }
}