using Autofac.Extras.Moq;
using Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline;
using TGramWeb.Services.MessageClient;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.Pipeline
{
    public class PipelineFailureRequestProcessorTest
    {
        private readonly ITestOutputHelper output;

        public PipelineFailureRequestProcessorTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_Process_Returns_NoResult_If_ObjectKind_Not_Supported()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject {[GitlabKeys.ObjectKind] = "some-unknown-kind"});

                Assert.False(result.Success);
                Assert.True(result.NoResult);
                Assert.Null(result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Returns_Failure_If_ObjectAttributes_Missing()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject {[GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline});

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected = $"1. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"";
                Assert.Equal(expected, result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Returns_Failure_If_ObjectAttributesStatus_Missing()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline,
                    [GitlabKeys.ObjectAttributes] = new JObject()
                });

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected = $"1. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Status}\"";
                Assert.Equal(expected, result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Returns_NoResult_If_ObjectAttributesStatus_Not_Supported()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline,
                    [GitlabKeys.ObjectAttributes] = new JObject{[GitlabKeys.Status] = "some-unsupported-status"}
                });

                Assert.False(result.Success);
                Assert.True(result.NoResult);
                Assert.Null(result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Calls_MessageClient_If_The_Message_Was_Formatted()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                string mockFormatterResult = "some-result";
                autoMock.Mock<IPipelineMessageFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateSuccess());

                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline,
                    [GitlabKeys.ObjectAttributes] = new JObject {[GitlabKeys.Status] = GitlabKeys.StatusFailed}
                };
                RequestProcessResult result = processor.Process(request);

                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                autoMock.Mock<IPipelineMessageFormatter>()
                        .Verify(p => p.TryFormat(request, out mockFormatterResult), Times.Once);

                autoMock.Mock<IMessageClient>()
                        .Verify(p => p.ScheduleDelivery(mockFormatterResult), Times.Once);
            }
        }

        [Fact]
        public void Test_Process_Does_Not_Call_MessageClient_If_The_Message_Was_Not_Formatted()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                string mockFormatterResult;
                autoMock.Mock<IPipelineMessageFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateFailure("some-reason"));

                var processor = autoMock.Create<PipelineFailureGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindPipeline,
                    [GitlabKeys.ObjectAttributes] = new JObject {[GitlabKeys.Status] = GitlabKeys.StatusFailed}
                };
                RequestProcessResult result = processor.Process(request);

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                Assert.Equal("some-reason", result.Reason);

                autoMock.Mock<IPipelineMessageFormatter>()
                        .Verify(p => p.TryFormat(request, out mockFormatterResult), Times.Once);

                autoMock.Mock<IMessageClient>()
                        .Verify(p => p.ScheduleDelivery(It.IsAny<string>()), Times.Never);
            }
        }
    }
}
