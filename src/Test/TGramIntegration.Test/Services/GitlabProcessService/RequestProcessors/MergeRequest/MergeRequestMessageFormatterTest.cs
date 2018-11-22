using System;
using Autofac.Extras.Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.MergeRequest;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.MergeRequest
{
    public class MergeRequestMessageFormatterTest
    {
        private readonly ITestOutputHelper output;

        public MergeRequestMessageFormatterTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_TryFormat_Should_Throw_If_Called_With_Illegal_Args()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var formatter = mock.Create<MergeRequestMessageFormatter>();
                void Caller() => formatter.TryFormat(null, out string msg);
                var ex = Assert.Throws<ArgumentNullException>((Action) Caller);
                Assert.Equal("request", ex.ParamName);
            }
        }

        [Fact]
        public void Test_TryFormat_Returns_Negative_If_Request_Has_No_Required_Attributes()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                var formatter = mock.Create<MergeRequestMessageFormatter>();

                RequestProcessResult result = formatter.TryFormat(new JObject(), out string msg1);
                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected1 = $"1. The json object is missing the field: \"{GitlabKeys.User}\"\r\n2. The json object is missing the field: \"{GitlabKeys.Assignee}\"\r\n3. The json object is missing the field: \"{GitlabKeys.Project}\"\r\n4. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"";
                Assert.Equal(expected1, result.Reason);


                var request2 = new JObject
                {
                    [GitlabKeys.User] = new JObject(),
                    [GitlabKeys.Assignee] = new JObject(),
                    [GitlabKeys.Project] = new JObject(),
                    [GitlabKeys.ObjectAttributes] = new JObject()
                };
                result = formatter.TryFormat(request2, out string msg2);
                Assert.False(result.Success);
                Assert.False(result.NoResult);

                string expected2 = $"1. The json object is missing the field: \"{GitlabKeys.User}.{GitlabKeys.Name}\"\r\n" +
                                   $"2. The json object is missing the field: \"{GitlabKeys.Assignee}.{GitlabKeys.Name}\"\r\n" +
                                   $"3. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.Name}\"\r\n" +
                                   $"4. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.WebUrl}\"\r\n" +
                                   $"5. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.SourceBranch}\"\r\n" +
                                   $"6. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.TargetBranch}\"\r\n" +
                                   $"7. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.State}\"\r\n" +
                                   $"8. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Title}\"\r\n" +
                                   $"9. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Url}\"\r\n" +
                                   $"10. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Iid}\"";
                Assert.Equal(expected2, result.Reason);
            }
        }

        [Theory]
        [InlineData(GitlabKeys.StateOpened, "created-at", "created-at", "opened")]
        [InlineData(GitlabKeys.StateOpened, "created-at", "updated-at", "updated")]
        [InlineData(GitlabKeys.StateClosed, "created-at", "updated-at", "closed")]
        public void Test_TryFormat_Returns_Positive_If_Message_Was_Formatted(string state, string createdAt, string updatedAt, string expectedText)
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                var formatter = mock.Create<MergeRequestMessageFormatter>();

                var request = new JObject
                {
                    [GitlabKeys.User] = new JObject{[GitlabKeys.Name] = "A-User-Name"},
                    [GitlabKeys.Assignee] = new JObject{[GitlabKeys.Name] = "An-Assignee-Name"},
                    [GitlabKeys.Project] = new JObject
                    {
                        [GitlabKeys.Name] = "A-Project-Name",
                        [GitlabKeys.WebUrl] = "A-Web-Url"
                    },
                    [GitlabKeys.ObjectAttributes] = new JObject
                    {
                        [GitlabKeys.SourceBranch] = "A-Source-Branch",
                        [GitlabKeys.TargetBranch] = "A-Target-Branch",
                        [GitlabKeys.State] = state,
                        [GitlabKeys.Title] = "A-Title",
                        [GitlabKeys.Url] = "A-Url",
                        [GitlabKeys.Iid] = "A-Iid",
                        [GitlabKeys.CreatedAt] = createdAt,
                        [GitlabKeys.UpdatedAt] = updatedAt
                    }
                };

                RequestProcessResult result = formatter.TryFormat(request, out string msg);
                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                string expected = $"[A-Project-Name](A-Web-Url). The merge request [#A-Iid A-Title](A-Url) (branch [A-Source-Branch](A-Web-Url/tree/A-Source-Branch) to [A-Target-Branch](A-Web-Url/tree/A-Target-Branch)) was {expectedText} by A-User-Name.\r\nAssignee *An-Assignee-Name*.";
                Assert.Equal(expected, msg);
            }
        }
    }
}
