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

        internal static ITestOutputHelper Output { get; private set; }

        public static IDisposable UseAsSharedSingleton(this ITestOutputHelper output)
        {
            OutputHelperExtensions.Output = output;
            return new OutputDispose();
        }

        private sealed class OutputDispose: IDisposable
        {
            public void Dispose()
            {
                OutputHelperExtensions.Output = null;
            }
        }
    }
}