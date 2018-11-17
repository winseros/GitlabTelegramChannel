using System;
using Autofac;
using Xunit.Abstractions;

namespace TGramTestUtil
{
    public static class OutputHelperExtensions
    {
        public static Action<ContainerBuilder> Capture(this ITestOutputHelper output)
        {
            return builder =>
            {
                builder.RegisterModule(new XUnitModule(output));
            };
        }
    }
}