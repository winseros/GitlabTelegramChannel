using System;
using TGramCommon.Exceptions;
using TGramDaemon.Services.MessageHandler;
using Xunit;

namespace TGramDaemon.Test.Services.MessageHandler
{
    public class MessageHandlerOptionsTest
    {
        public class ThrowIfInvalid
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void It_Should_Throw_If_Token_Is_Invalid(string address)
            {
                void Caller() => new MessageHandlerOptions {Address = address}.ThrowIfInvalid();
                var ex = Assert.Throws<ConfigurationException>((Action) Caller);
                const string expected = "The MessageHandlerOptions.Address setting is not configured";
                Assert.Equal(expected, ex.Message);
            }

            [Fact]
            public void It_Should_Not_Throw_If_Fields_Are_Valid()
            {
                new MessageHandlerOptions
                {
                    Address = "tcp://127.0.0.1:5000"
                }.ThrowIfInvalid();
            }
        }
    }
}
