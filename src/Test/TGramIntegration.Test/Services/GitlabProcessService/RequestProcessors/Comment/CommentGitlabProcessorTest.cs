using Autofac.Extras.Moq;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment;
using TGramWeb.Services.MessageClient;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentGitlabProcessorTest
    {
        private readonly ITestOutputHelper output;

        public CommentGitlabProcessorTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_Process_Returns_NoResult_If_ObjectKind_Not_Supported()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<MockGitlabProcessor>();
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
                var processor = autoMock.Create<MockGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject {[GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindNote});

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected = $"1. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"";
                Assert.Equal(expected, result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Returns_Failure_If_ObjectAttributesNoteableType_Missing()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                var processor = autoMock.Create<MockGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindNote,
                    [GitlabKeys.ObjectAttributes] = new JObject()
                });

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected = $"1. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.NoteableType}\"";
                Assert.Equal(expected, result.Reason);
            }
        }

        [Fact]
        public void Test_Process_Returns_NoResult_If_ObjectAttributesNoteableType_Not_Supported()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                autoMock.Mock<IMockCanHandle>()
                        .Setup(p => p.CanHandle(It.IsAny<string>()))
                        .Returns(false);

                var processor = autoMock.Create<MockGitlabProcessor>();
                RequestProcessResult result = processor.Process(new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindNote,
                    [GitlabKeys.ObjectAttributes] = new JObject{[GitlabKeys.NoteableType] = "some-unsupported-type"}
                });

                Assert.False(result.Success);
                Assert.True(result.NoResult);
                Assert.Null(result.Reason);

                autoMock.Mock<IMockCanHandle>()
                        .Verify(p => p.CanHandle("some-unsupported-type"), Times.Once);
            }
        }

        [Fact]
        public void Test_Process_Calls_MessageClient_If_The_Message_Was_Formatted()
        {
            using (AutoMock autoMock = AutoMock.GetLoose(this.output.Capture()))
            {
                autoMock.Mock<IMockCanHandle>()
                        .Setup(p => p.CanHandle(It.IsAny<string>()))
                        .Returns(true);

                string mockFormatterResult = "some-result";
                autoMock.Mock<IMockFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateSuccess());

                var processor = autoMock.Create<MockGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindNote,
                    [GitlabKeys.ObjectAttributes] = new JObject {[GitlabKeys.NoteableType] = GitlabKeys.NoteableTypeSnippet}
                };
                RequestProcessResult result = processor.Process(request);

                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                autoMock.Mock<IMockCanHandle>()
                        .Verify(p => p.CanHandle(GitlabKeys.NoteableTypeSnippet), Times.Once);

                autoMock.Mock<IMockFormatter>()
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
                autoMock.Mock<IMockCanHandle>()
                        .Setup(p => p.CanHandle(It.IsAny<string>()))
                        .Returns(true);

                string mockFormatterResult;
                autoMock.Mock<IMockFormatter>()
                        .Setup(p => p.TryFormat(It.IsAny<JObject>(), out mockFormatterResult))
                        .Returns(RequestProcessResult.CreateFailure("some-reason"));

                var processor = autoMock.Create<MockGitlabProcessor>();
                var request = new JObject
                {
                    [GitlabKeys.ObjectKind] = GitlabKeys.ObjectKindNote,
                    [GitlabKeys.ObjectAttributes] = new JObject {[GitlabKeys.NoteableType] = GitlabKeys.NoteableTypeSnippet}
                };
                RequestProcessResult result = processor.Process(request);

                Assert.False(result.Success);
                Assert.False(result.NoResult);
                Assert.Equal("some-reason", result.Reason);

                autoMock.Mock<IMockCanHandle>()
                        .Verify(p => p.CanHandle(GitlabKeys.NoteableTypeSnippet), Times.Once);

                autoMock.Mock<IMockFormatter>()
                        .Verify(p => p.TryFormat(request, out mockFormatterResult), Times.Once);

                autoMock.Mock<IMessageClient>()
                        .Verify(p => p.ScheduleDelivery(It.IsAny<string>()), Times.Never);
            }
        }

        public class MockGitlabProcessor: CommentGitlabProcessor
        {
            private readonly IMockFormatter formatter;
            private readonly IMockCanHandle canHandle;

            public MockGitlabProcessor(IMockFormatter formatter,
                                       IMockCanHandle canHandle,
                                       IMessageClient messageClient,
                                       ILogger<CommentGitlabProcessor> logger)
                : base(messageClient, logger)
            {
                this.formatter = formatter;
                this.canHandle = canHandle;
            }

            protected override bool CanHandle(string noteableType)
            {
                return this.canHandle.CanHandle(noteableType);
            }

            protected override RequestProcessResult TryFormat(JObject request, out string message)
            {
                return this.formatter.TryFormat(request, out message);
            }
        }

        public interface IMockFormatter
        {
            RequestProcessResult TryFormat(JObject request, out string message);
        }

        public interface IMockCanHandle
        {
           bool CanHandle(string noteableType);
        }
    }
}
