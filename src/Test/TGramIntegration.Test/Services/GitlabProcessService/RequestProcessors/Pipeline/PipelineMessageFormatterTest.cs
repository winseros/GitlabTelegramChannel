using System;
using Autofac.Extras.Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.Pipeline
{
    public class PipelineMessageFormatterTest
    {
        private readonly ITestOutputHelper output;

        public PipelineMessageFormatterTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_TryFormat_Should_Throw_If_Called_With_Illegal_Args()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var formatter = mock.Create<PipelineMessageFormatter>();
                void Caller() => formatter.TryFormat(null, out string _);
                var ex = Assert.Throws<ArgumentNullException>((Action) Caller);
                Assert.Equal("request", ex.ParamName);
            }
        }

        [Fact]
        public void Test_TryFormat_Returns_Negative_If_Request_Has_No_Required_Attributes()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                var formatter = mock.Create<PipelineMessageFormatter>();

                RequestProcessResult result = formatter.TryFormat(new JObject(), out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected1 = $"1. The json object is missing the field: \"{GitlabKeys.Project}\"\r\n2. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"";
                Assert.Equal(expected1, result.Reason);


                var request2 = new JObject {[GitlabKeys.Project] = new JObject(), [GitlabKeys.ObjectAttributes] = new JObject()};
                result = formatter.TryFormat(request2, out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected2 = $"1. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.Name}\"\r\n2. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.WebUrl}\"\r\n3. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Ref}\"\r\n4. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Id}\"";
                Assert.Equal(expected2, result.Reason);
            }
        }

        [Fact]
        public void Test_TryFormat_Returns_Positive_If_Message_Was_Formatted()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                var formatter = mock.Create<PipelineMessageFormatter>();

                var request = new JObject
                {
                    [GitlabKeys.Project] = new JObject
                    {
                        [GitlabKeys.Name] = "A-Project-Name",
                        [GitlabKeys.WebUrl] = "A-Web-Url"
                    },
                    [GitlabKeys.ObjectAttributes] = new JObject
                    {
                        [GitlabKeys.Ref] = "A-Ref",
                        [GitlabKeys.Id] = "A-Id"
                    }
                };

                RequestProcessResult result = formatter.TryFormat(request, out string msg);
                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                const string expected = "[A-Project-Name](A-Web-Url). The pipeline [A-Id](A-Web-Url/pipelines/A-Id) has failed for the branch [A-Ref](A-Web-Url/tree/A-Ref)!";
                Assert.Equal(expected, msg);
            }
        }
    }
}
