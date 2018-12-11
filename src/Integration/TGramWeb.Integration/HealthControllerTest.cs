using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TGramTestUtil;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Integration
{
    public class HealthControllerTest
    {
        private readonly ITestOutputHelper output;

        public HealthControllerTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Test_Get_Index_Returns_Ok()
        {
            using (this.output.UseAsSharedSingleton())
            using (var cts = new CancellationTokenSource())
            {
                Task start = IntegrationUtils.StartApplicationAsync(5000, cts.Token);

                await IntegrationUtils.WaitForThePortAcquired(5000);
                HttpResponseMessage message = await IntegrationUtils.HttpGetAsync(new Uri("http://localhost:5000"));

                Assert.Equal(HttpStatusCode.OK, message.StatusCode);

                cts.Cancel();
                await start;
            }
        }
    }
}
