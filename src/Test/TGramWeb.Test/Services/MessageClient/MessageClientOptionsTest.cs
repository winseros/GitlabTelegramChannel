using System;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramWeb.Services.MessageClient;
using Xunit;

namespace TGramWeb.Test.Services.MessageClient
{
    public class MessageClientOptionsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Test_ThrowIfInvalid_Should_Throw_If_Address_Is_Invalid(string address)
        {
            void Caller() => new MessageClientOptions {Address = address}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"The \"{ConfigKeys.Daemon}:{nameof(MessageClientOptions.Address)}\" setting is not configured";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Test_ThrowIfInvalid_Should_Not_Throw_If_Fields_Are_Valid()
        {
            new MessageClientOptions
            {
                Address = "tcp://127.0.0.1:5000"
            }.ThrowIfInvalid();
        }
    }
}
