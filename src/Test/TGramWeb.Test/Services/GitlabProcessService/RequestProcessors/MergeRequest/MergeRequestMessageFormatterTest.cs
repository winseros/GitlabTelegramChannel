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
                var formatter = mock.Create<MergeRequestMessageFormatter>();

                RequestProcessResult result = formatter.TryFormat(new JObject(), out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected1 = $"1. The json object is missing the field: \"{GitlabKeys.User}\"{Environment.NewLine}" +
                                   $"2. The json object is missing the field: \"{GitlabKeys.Assignee}\"{Environment.NewLine}" +
                                   $"3. The json object is missing the field: \"{GitlabKeys.Project}\"{Environment.NewLine}" +
                                   $"4. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"";
                Assert.Equal(expected1, result.Reason);


                var request2 = new JObject
                {
                    [GitlabKeys.User] = new JObject(),
                    [GitlabKeys.Assignee] = new JObject(),
                    [GitlabKeys.Project] = new JObject(),
                    [GitlabKeys.ObjectAttributes] = new JObject()
                };
                result = formatter.TryFormat(request2, out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);

                string expected2 = $"1. The json object is missing the field: \"{GitlabKeys.User}.{GitlabKeys.Name}\"{Environment.NewLine}" +
                                   $"2. The json object is missing the field: \"{GitlabKeys.Assignee}.{GitlabKeys.Name}\"{Environment.NewLine}" +
                                   $"3. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.Name}\"{Environment.NewLine}" +
                                   $"4. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.WebUrl}\"{Environment.NewLine}" +
                                   $"5. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.SourceBranch}\"{Environment.NewLine}" +
                                   $"6. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.TargetBranch}\"{Environment.NewLine}" +
                                   $"7. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.State}\"{Environment.NewLine}" +
                                   $"8. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Title}\"{Environment.NewLine}" +
                                   $"9. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Url}\"{Environment.NewLine}" +
                                   $"10. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Iid}\"";
                Assert.Equal(expected2, result.Reason);
            }
        }

        [Theory]
        [InlineData(GitlabKeys.StateOpened, "2018-05-10T10:20:25", "2018-05-10T10:20:25", "opened")]
        [InlineData(GitlabKeys.StateOpened, "2018-05-10T10:20:25", "2018-05-10T10:20:26", "opened")]
        [InlineData(GitlabKeys.StateOpened, "2018-05-10T10:20:25", "2018-05-10T10:59:59", "updated")]
        [InlineData(GitlabKeys.StateClosed, "2018-05-10T10:20:25", "2018-05-10T10:59:59", "closed")]
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
