using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TGramTestUtil;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Integration
{
    public class WebIntegrationTest
    {
        private readonly ITestOutputHelper output;

        public WebIntegrationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Test_It_Starts_Without_Errors()
        {
            using (this.output.UseAsSharedSingleton())
            using (var cts = new CancellationTokenSource())
            {
                Task start = IntegrationUtils.StartApplication(5000, cts.Token);

                HttpResponseMessage message = await IntegrationUtils.HttpGetAsync(new Uri("http://localhost:5000/health"));

                Assert.Equal(HttpStatusCode.OK, message.StatusCode);

                cts.Cancel();
                await start;
            }
            
        }

    }
}
