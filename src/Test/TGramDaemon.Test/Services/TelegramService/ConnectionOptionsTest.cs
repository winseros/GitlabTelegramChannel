using System;
using System.Collections.Generic;
using System.Text;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramDaemon.Services.TelegramService;
using Xunit;

namespace TGramDaemon.Test.Services.TelegramService
{
    public class ConnectionOptionsTest
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void It_Should_Throw_If_Timeout_Is_Invalid(short timeout)
        {
            void Caller() => new ConnectionOptions {Timeout = timeout, Attempts = 1, Interval = 1}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"1. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Timeout)}\" must be a positive number";
            Assert.Equal(expected, ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void It_Should_Throw_If_Attempts_Is_Invalid(short attempts)
        {
            void Caller() => new ConnectionOptions {Timeout = 1, Attempts = attempts, Interval = 1}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"1. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Attempts)}\" must be a positive number";
            Assert.Equal(expected, ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void It_Should_Throw_If_Interval_Is_Invalid(short interval)
        {
            void Caller() => new ConnectionOptions {Timeout = 1, Attempts = 1, Interval = interval}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"1. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Interval)}\" must be a positive number";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void It_Should_Throw_If_Multiple_Fields_Are_Invalid()
        {
            void Caller() => new ConnectionOptions{Timeout = -1, Attempts = -1, Interval = -1}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"1. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Timeout)}\" must be a positive number{Environment.NewLine}" +
                              $"2. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Attempts)}\" must be a positive number{Environment.NewLine}" +
                              $"3. The \"{ConfigKeys.TelegramConnection}:{nameof(ConnectionOptions.Interval)}\" must be a positive number";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void It_Should_Not_Throw_If_All_Fields_Are_Valid()
        {
            new ConnectionOptions().ThrowIfInvalid();
        }
    }
}
