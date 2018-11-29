using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using TGramCommon;
using TGramWeb.Authentication;
using Xunit;

namespace TGramWeb.Test.Authentication
{
    public class ConfigurationExtensionsTest
    {
        [Fact]
        public void Test_InflateGitlabOptions_Inflates_Options()
        {
            var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource
            {
                InitialData = new[] {new KeyValuePair<string, string>($"{ConfigKeys.Gitlab}:{nameof(GitlabAuthenticationOptions.Token)}", "12345")}
            }).Build();

            var options = new GitlabAuthenticationOptions();
            config.InflateGitlabOptions(options);

            Assert.Equal("12345", options.Token);
        }
    }
}
