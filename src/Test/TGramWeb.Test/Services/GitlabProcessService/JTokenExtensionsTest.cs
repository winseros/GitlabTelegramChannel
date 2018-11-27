using System;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService
{
    public class JTokenExtensionsTest
    {
        [Fact]
        public void Test_RequireString_Throws_If_Called_With_Illegal_Args()
        {
            void Caller1() => ((JObject) null).RequireString(null, null);
            ArgumentException ex = Assert.Throws<ArgumentNullException>((Action) Caller1);
            Assert.Equal("source", ex.ParamName);

            void Caller2() => new JObject().RequireString(null, null);
            ex = Assert.Throws<ArgumentException>((Action) Caller2);
            Assert.Equal("key", ex.Message);

            void Caller3() => new JObject().RequireString("abcd", null);
            ex = Assert.Throws<ArgumentNullException>((Action) Caller3);
            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Test_RequireString_Returns_An_Existing_String()
        {
            var errors = new JTokenErrors();
            var container = new JObject
            {
                ["prop1"] = "abcd"
            };
            string str1 = container.RequireString("prop1", errors);

            Assert.Equal("abcd", str1);
            Assert.False(errors.HasAny);
        }

        [Fact]
        public void Test_RequireString_Returns_Errors_For_Plain_Objects()
        {
            var errors = new JTokenErrors();
            var container = new JObject();
            string str1 = container.RequireString("prop1", errors);

            Assert.Null(str1);
            Assert.True(errors.HasAny);

            const string expected = "1. The json object is missing the field: \"prop1\"";
            string error = errors.Compose();
            Assert.Equal(expected, error);
        }

        [Fact]
        public void Test_RequireString_Returns_Errors_For_Nested_Objects()
        {
            var errors = new JTokenErrors();
            var container = new JObject {["child"] = new JObject()};
            string str1 = container["child"].RequireString("prop1", errors);

            Assert.Null(str1);
            Assert.True(errors.HasAny);

            const string expected = "1. The json object is missing the field: \"child.prop1\"";
            string error = errors.Compose();
            Assert.Equal(expected, error);
        }
    }
}
