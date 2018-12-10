using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
                //arrange
                Task start = IntegrationUtils.StartApplicationAsync(5000, cts.Token);
                Task listen = listener.StartListenerAsync(cts.Token);

                //act
                var uri = new Uri("http://localhost:5000/gitlab_hook");
                object request = GitlabControllerTest.CreatePipelineFailedRequest();
                HttpResponseMessage message = await IntegrationUtils.HttpPostAsync(uri, request);

                //assert - OK returned
                Assert.Equal(HttpStatusCode.OK, message.StatusCode);

                //assert - Telegram message was sent
                HttpRequest tgRequest = await listener.GetCapturedRequestAsync();
                Assert.Equal("/bota-telegram-token/sendMessage" ,tgRequest.Path);

                using (var reader = new StreamReader(tgRequest.Body))
                {
                    string body = reader.ReadToEnd();
                    const string expected = "{\"chat_id\":\"a-telegram-channel\",\"text\":\"[A sample project](https://gitlab.com). The pipeline [10](https://gitlab.com/pipelines/10) has failed for the branch [master](https://gitlab.com/tree/master)!\",\"parse_mode\":\"Markdown\"}";
                    Assert.Equal(expected, body);
                }

                //shutdown the servers
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
