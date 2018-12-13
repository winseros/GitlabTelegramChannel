using System;
using Autofac.Extras.Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService.RequestProcessors.Comment
{
    public class CommentRequestMessageFormatterTest
    {
        private readonly ITestOutputHelper output;

        public CommentRequestMessageFormatterTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_TryFormat_Should_Throw_If_Called_With_Illegal_Args()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var formatter = mock.Create<CommentRequestMessageFormatter>();
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
                var formatter = mock.Create<CommentRequestMessageFormatter>();

                RequestProcessResult result = formatter.TryFormat(new JObject(), out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);
                string expected1 = $"1. The json object is missing the field: \"{GitlabKeys.User}\"{Environment.NewLine}" +
                                   $"2. The json object is missing the field: \"{GitlabKeys.Project}\"{Environment.NewLine}" +
                                   $"3. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}\"{Environment.NewLine}" +
                                   $"4. The json object is missing the field: \"{GitlabKeys.MergeRequest}\"";
                Assert.Equal(expected1, result.Reason);


                var request2 = new JObject
                {
                    [GitlabKeys.User] = new JObject(),
                    [GitlabKeys.Project] = new JObject(),
                    [GitlabKeys.ObjectAttributes] = new JObject(),
                    [GitlabKeys.MergeRequest] = new JObject()
                };
                result = formatter.TryFormat(request2, out string _);
                Assert.False(result.Success);
                Assert.False(result.NoResult);

                string expected2 = $"1. The json object is missing the field: \"{GitlabKeys.User}.{GitlabKeys.Name}\"{Environment.NewLine}" +
                                   $"2. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.Name}\"{Environment.NewLine}" +
                                   $"3. The json object is missing the field: \"{GitlabKeys.Project}.{GitlabKeys.WebUrl}\"{Environment.NewLine}" +
                                   $"4. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Note}\"{Environment.NewLine}" +
                                   $"5. The json object is missing the field: \"{GitlabKeys.ObjectAttributes}.{GitlabKeys.Url}\"{Environment.NewLine}" +
                                   $"6. The json object is missing the field: \"{GitlabKeys.MergeRequest}.{GitlabKeys.Title}\"{Environment.NewLine}" +
                                   $"7. The json object is missing the field: \"{GitlabKeys.MergeRequest}.{GitlabKeys.Iid}\"";
                Assert.Equal(expected2, result.Reason);
            }
        }

        [Theory]
        [InlineData("2018-05-10T10:20:25", "2018-05-10T10:20:25", "commented")]
        [InlineData("2018-05-10T10:20:25", "2018-05-10T10:20:26", "commented")]
        [InlineData("2018-05-10T10:20:25", "2018-05-10T10:59:59", "updated the comment")]
        public void Test_TryFormat_Returns_Positive_If_Message_Was_Formatted(string createdAt, string updatedAt, string expectedText)
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                var formatter = mock.Create<CommentRequestMessageFormatter>();

                var request = new JObject
                {
                    [GitlabKeys.User] = new JObject{[GitlabKeys.Name] = "A-User-Name"},
                    [GitlabKeys.Project] = new JObject
                    {
                        [GitlabKeys.Name] = "A-Project-Name",
                        [GitlabKeys.WebUrl] = "A-Web-Url"
                    },
                    [GitlabKeys.ObjectAttributes] = new JObject
                    {
                        [GitlabKeys.Note] = "A-Note",
                        [GitlabKeys.Url] = "A-Url",
                        [GitlabKeys.CreatedAt] = createdAt,
                        [GitlabKeys.UpdatedAt] = updatedAt
                    },
                    [GitlabKeys.MergeRequest] = new JObject
                    {
                        [GitlabKeys.Title] = "A-Title",
                        [GitlabKeys.Iid] = "A-Iid"
                    }
                };

                RequestProcessResult result = formatter.TryFormat(request, out string msg);
                Assert.True(result.Success);
                Assert.False(result.NoResult);
                Assert.Null(result.Reason);

                string expected = $"[A-Project-Name](A-Web-Url). *A-User-Name* has [{expectedText}](A-Url) on the MR [#A-Iid A-Title](A-Web-Url/merge_requests/A-Iid)!\r\n\r\nA-Note";
                Assert.Equal(expected, msg);
            }
        }
    }
}
