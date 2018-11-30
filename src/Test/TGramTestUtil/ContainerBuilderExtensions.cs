using Autofac;
using Xunit.Abstractions;

namespace TGramTestUtil
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterXUnit(this ContainerBuilder builder, ITestOutputHelper output)
        {
            builder.RegisterModule(new XUnitModule(output));
        }
    }
}
