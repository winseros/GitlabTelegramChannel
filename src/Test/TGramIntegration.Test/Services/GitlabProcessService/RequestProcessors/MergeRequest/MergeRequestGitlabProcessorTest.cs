using Autofac.Extras.Moq;
using Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.MergeRequest;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline;
using TGramWeb.Services.MessageClient;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.MergeRequest
{
    public class MergeRequestGitlabProcessorTest
    {
        private readonly ITestOutputHelper output;

        public MergeRequestGitlabProcessorTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_Process_Returns_NoResult_If_ObjectKind_Not_Supported()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<MergeRequestGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject {[GitlabKeys.ObjectKind] = "some-unknown-kind"});

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
                var mockFormatterResult = "some-result";
                autoMock.Mock<IMergeRequestMessageFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateSuccess());

                var processor = autoMock.Create<MergeRequestGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindMergeRequest
                };
                RequestProcessResult result = processor.Process(request);

                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                autoMock.Mock<IMergeRequestMessageFormatter>()
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
                autoMock.Mock<IMergeRequestMessageFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateFailure("some-reason"));

                var processor = autoMock.Create<MergeRequestGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindMergeRequest
                };
                RequestProcessResult result = processor.Process(request);

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                Assert.Equal("some-reason", result.Reason);

                autoMock.Mock<IMergeRequestMessageFormatter>()
                        .Verify(p => p.TryFormat(request, out mockFormatterResult), Times.Once);

                autoMock.Mock<IMessageClient>()
                        .Verify(p => p.ScheduleDelivery(It.IsAny<string>()), Times.Never);
            }
        }
    }
}
