using System;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.Models;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService.Models
{
    public class UserInfoTest
    {
        [Fact]
        public void Test_TryRead_Should_Throw_If_Called_With_Illegal_Args()
        {
            void Caller() => UserInfo.TryRead(new JObject(), null);
            var ex = Assert.Throws<ArgumentNullException>((Action) Caller);
            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Test_TryRead_Returns_Null_If_Commit_Is_Null()
        {
            var errors = new JTokenErrors();
            UserInfo info = UserInfo.TryRead(null, errors);

            Assert.Null(info);
            Assert.False(errors.HasAny);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("some-random-name", null)]
        [InlineData(null, "commit-url")]
        public void Test_TryRead_Should_Not_Read_If_Commit_Lacks_Data(string name, string username)
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = name,
                [GitlabKeys.UserName] = username
            };

            var errors = new JTokenErrors();
            UserInfo info = UserInfo.TryRead(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);
        }

        [Fact]
        public void Test_TryRead_Should_Read_UserInfo()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "name",
                [GitlabKeys.UserName] = "user-name"
            };

            var errors = new JTokenErrors();
            UserInfo info = UserInfo.TryRead(data, errors);

            Assert.False(errors.HasAny);

            Assert.Equal("name", info.Name);
            Assert.Equal("user-name", info.UserName);
        }
    }
}
