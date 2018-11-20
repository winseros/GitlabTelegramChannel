using System;
using TGramWeb.Services.GitlabProcessService;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService
{
    public class JTokenErrorsTest
    {
        [Fact]
        public void Test_JTokenErrors_Functional()
        {
            var errors = new JTokenErrors();
            Assert.False(errors.HasAny);

            errors.Add("error text 1");
            Assert.True(errors.HasAny);

            string text1 = errors.Compose();
            Assert.Equal("1. error text 1", text1);

            errors.Add("error text 2");
            Assert.True(errors.HasAny);

            string text2 = errors.Compose();
            Assert.Equal($"1. error text 1{Environment.NewLine}2. error text 2", text2);
        }
    }
}
