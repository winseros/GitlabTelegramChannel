using System;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramWeb.Authentication;
using Xunit;

namespace TGramWeb.Test.Authentication
{
    public class GitlabAuthenticationOptionsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Test_ThrowIfInvalid_Throws_If_Token_Is_Missing(string token)
        {
            void Caller() => new GitlabAuthenticationOptions {Token = token}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"The \"{ConfigKeys.Gitlab}:{nameof(GitlabAuthenticationOptions.Token)}\" is not configured";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Test_ThrowIfInvalid_Does_Not_Throw_If_Token_Is_Present()
        {
            new GitlabAuthenticationOptions
            {
                Token = "12345"
            }.ThrowIfInvalid();
        }
    }
}
