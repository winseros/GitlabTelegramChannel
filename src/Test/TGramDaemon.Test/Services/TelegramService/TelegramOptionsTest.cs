using System;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramDaemon.Services.TelegramService;
using Xunit;

namespace TGramDaemon.Test.Services.TelegramService
{
    public class TelegramOptionsTest
    {
        public class ThrowIfInvalid
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void It_Should_Throw_If_Token_Is_Invalid(string token)
            {
                void Caller() => new TelegramOptions {Token = token, Channel = "abc", Endpoint = new Uri("https://example.com")}.ThrowIfInvalid();
                var ex = Assert.Throws<ConfigurationException>((Action) Caller);
                string expected = $"1. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Token)}\" setting not configured";
                Assert.Equal(expected, ex.Message);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void It_Should_Throw_If_Channel_Is_Invalid(string channel)
            {
                void Caller() => new TelegramOptions {Token = "vbgau", Channel = channel, Endpoint = new Uri("https://example.com")}.ThrowIfInvalid();
                var ex = Assert.Throws<ConfigurationException>((Action) Caller);
                string expected = $"1. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Channel)}\" setting not configured";
                Assert.Equal(expected, ex.Message);
            }

            [Fact]
            public void It_Should_Throw_If_Endpoint_Is_Invalid()
            {
                void Caller() => new TelegramOptions {Token = "vbgau", Channel = "abc"}.ThrowIfInvalid();
                var ex = Assert.Throws<ConfigurationException>((Action) Caller);
                string expected = $"1. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Endpoint)}\" setting not configured";
                Assert.Equal(expected, ex.Message);
            }

            [Fact]
            public void It_Should_Throw_If_Multiple_Fields_Are_Invalid()
            {
                void Caller() => new TelegramOptions().ThrowIfInvalid();
                var ex = Assert.Throws<ConfigurationException>((Action) Caller);
                string expected = $"1. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Token)}\" setting not configured{Environment.NewLine}" +
                                  $"2. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Channel)}\" setting not configured{Environment.NewLine}" +
                                  $"3. The \"{ConfigKeys.Telegram}:{nameof(TelegramOptions.Endpoint)}\" setting not configured";
                Assert.Equal(expected, ex.Message);
            }

            [Fact]
            public void It_Should_Not_Throw_If_Fields_Are_Valid()
            {
                new TelegramOptions
                {
                    Token = "bhcau",
                    Channel = "bhjkl",
                    Endpoint = new Uri("https://example.com")
                }.ThrowIfInvalid();
            }
        }
    }
}
