using System;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramDaemon.Services.MessageHandler;
using Xunit;

namespace TGramDaemon.Test.Services.MessageHandler
{
    public class MessageHandlerOptionsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Test_ThrowIfInvalid_Should_Throw_If_Token_Is_Invalid(string address)
        {
            void Caller() => new MessageHandlerOptions {Address = address}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"The \"{ConfigKeys.Daemon}:{nameof(MessageHandlerOptions.Address)}\" setting is not configured";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Test_ThrowIfInvalid_Should_Not_Throw_If_Fields_Are_Valid()
        {
            new MessageHandlerOptions
            {
                Address = "tcp://127.0.0.1:5000"
            }.ThrowIfInvalid();
        }
    }
}
