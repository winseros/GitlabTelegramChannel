using System;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.Models;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService.Models
{
    public class CommitInfoTest
    {
        [Fact]
        public void Test_TryRead_Should_Throw_If_Called_With_Illegal_Args()
        {
            void Caller() => CommitInfo.TryRead(new JObject(), null);
            var ex = Assert.Throws<ArgumentNullException>((Action) Caller);
            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Test_TryRead_Returns_Null_If_Commit_Is_Null()
        {
            var errors = new JTokenErrors();
            CommitInfo info = CommitInfo.TryRead(null, errors);

            Assert.Null(info);
            Assert.False(errors.HasAny);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("70jhoce89ca", null, null)]
        [InlineData(null, "commmit-url", null)]
        [InlineData(null, null, "a-message")]
        [InlineData("70jhoce89ca", "commmit-url", "a-message")]
        public void Test_TryRead_Should_Not_Read_If_Commit_Lacks_Data(string hash, string url, string msg)
        {
            var data = new JObject
            {
                [GitlabKeys.Id] = hash,
                [GitlabKeys.Url] = url,
                [GitlabKeys.Message] = msg
            };

            var errors = new JTokenErrors();
            CommitInfo info = CommitInfo.TryRead(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("user-name", null)]
        [InlineData(null, "user-email")]
        public void Test_TryRead_Should_Not_Read_If_Commit_Lacks_Author_Data(string name, string email)
        {
            var data = new JObject
            {
                [GitlabKeys.Id] = "4567889",
                [GitlabKeys.Url] = "http://example.com",
                [GitlabKeys.Message] = "a-message",
                [GitlabKeys.Author] = new JObject
                {
                    [GitlabKeys.Name] = name,
                    [GitlabKeys.Email] = email
                }
            };

            var errors = new JTokenErrors();
            CommitInfo info = CommitInfo.TryRead(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);
        }

        [Fact]
        public void Test_TryRead_Should_Read_CommitInfo()
        {
            var data = new JObject
            {
                [GitlabKeys.Id] = "4567889abhjcksalcy8a79",
                [GitlabKeys.Url] = "http://example.com",
                [GitlabKeys.Message] = "a-message",
                [GitlabKeys.Author] = new JObject
                {
                    [GitlabKeys.Name] = "a-name",
                    [GitlabKeys.Email] = "an-email"
                }
            };

            var errors = new JTokenErrors();
            CommitInfo info = CommitInfo.TryRead(data, errors);

            Assert.False(errors.HasAny);

            Assert.Equal("4567889", info.Hash);
            Assert.Equal("http://example.com", info.Url);
            Assert.Equal("a-message", info.Message);
            Assert.Equal("a-name", info.AuthorName);
            Assert.Equal("an-email", info.AuthorEmail);
        }
    }
}
