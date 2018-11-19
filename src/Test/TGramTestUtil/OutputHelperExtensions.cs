using System;
using Autofac;
using Xunit.Abstractions;

namespace TGramTestUtil
{
    public static class OutputHelperExtensions
    {
        public static Action<ContainerBuilder> Capture(this ITestOutputHelper output)
        {
            return builder => builder.RegisterXUnit(output);
        }

        public static void RegisterXUnit(this ContainerBuilder builder, ITestOutputHelper output)
        {
            builder.RegisterModule(new XUnitModule(output));
        }
    }
}