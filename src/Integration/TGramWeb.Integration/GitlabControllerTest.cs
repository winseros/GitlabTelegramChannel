using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Integration
{
    public class GitlabControllerTest
    {
        private readonly ITestOutputHelper output;

        public GitlabControllerTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Test_Get_Index_Processes_Requests()
        {
            using (this.output.UseAsSharedSingleton())
            using (var cts = new CancellationTokenSource())
            using (IWebHost listener = IntegrationUtils.CreateListener())
            {
                Task start = IntegrationUtils.StartApplicationAsync(5000, cts.Token);
                Task listen = listener.StartListenerAsync(cts.Token);

                var uri = new Uri("http://localhost:5000/gitlab_hook");
                object request = GitlabControllerTest.CreatePipelineFailedRequest();
                HttpResponseMessage message = await IntegrationUtils.HttpPostAsync(uri, request);

                Assert.Equal(HttpStatusCode.OK, message.StatusCode);

                HttpRequest data = await listener.GetCapturedRequestAsync();
                Assert.Equal("/bota-telegram-bot/sendMessage" ,data.Path);

                cts.Cancel();
                await Task.WhenAll(start, listen);
            }
        }

        private static object CreatePipelineFailedRequest()
        {
            IDictionary<string, object> project = new ExpandoObject();
            project[GitlabKeys.Name] = "A sample project";
            project[GitlabKeys.WebUrl] = "https://gitlab.com";

            IDictionary<string, object> attributes = new ExpandoObject();
            attributes[GitlabKeys.Status] = GitlabKeys.StatusFailed;
            attributes[GitlabKeys.Ref] = "master";
            attributes[GitlabKeys.Id] = 10;

            IDictionary<string, object> data = new ExpandoObject();
            data[GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline;
            data[GitlabKeys.Project] = project;
            data[GitlabKeys.ObjectAttributes] = attributes;

            return data;
        }

        
    }
}
