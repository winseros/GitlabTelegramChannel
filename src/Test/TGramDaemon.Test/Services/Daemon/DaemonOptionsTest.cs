using System;
using TGramCommon;
using TGramCommon.Exceptions;
using TGramDaemon.Services.Daemon;
using Xunit;

namespace TGramDaemon.Test.Services.Daemon
{
    public class DaemonOptionsTest
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Test_ThrowIfInvalid_Should_Throw_If_ThreadCount_Is_Invalid(short count)
        {
            void Caller() => new DaemonOptions {ThreadCount = count}.ThrowIfInvalid();
            var ex = Assert.Throws<ConfigurationException>((Action) Caller);
            string expected = $"The \"{ConfigKeys.Daemon}:{nameof(DaemonOptions.ThreadCount)}\" must be a positive number";
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Test_ThrowIfInvalid_Should_Not_Throw_If_Fields_Are_Valid()
        {
            new DaemonOptions
            {
                ThreadCount = 1
            }.ThrowIfInvalid();
        }
    }
}
