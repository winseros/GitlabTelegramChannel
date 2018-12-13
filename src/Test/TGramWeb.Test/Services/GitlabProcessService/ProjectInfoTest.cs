using System;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService
{
    public class ProjectInfoTest
    {
        [Fact]
        public void Test_Read_Should_Throw_If_Called_With_Illegal_Args()
        {
            void Caller1() => ProjectInfo.Read(null, null);
            var ex = Assert.Throws<ArgumentNullException>((Action) Caller1);
            Assert.Equal("project", ex.ParamName);

            void Caller2() => ProjectInfo.Read(new JObject(), null);
            ex = Assert.Throws<ArgumentNullException>((Action) Caller2);
            Assert.Equal("errors", ex.ParamName);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("project-name", null, null)]
        [InlineData(null, "project-url", null)]
        [InlineData(null, null, "path")]
        public void Test_Read_Should_Not_Read_If_Project_Lacks_Data(string projectName, string projectUrl, string path)
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = projectName,
                [GitlabKeys.WebUrl] = projectUrl,
                [GitlabKeys.PathWithNamespace] = path
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.Read(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);
        }

        [Fact]
        public void Test_Read_Should_Not_Read_If_Could_Not_Retrieve_Server_Url_Plain()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "a-project-name",
                [GitlabKeys.WebUrl] = "a-web-url",
                [GitlabKeys.PathWithNamespace] = "a-very-long-path"
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.Read(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);

            const string expectedError = "1. Can not retrieve the server url out of \"web_url\"";
            Assert.Equal(expectedError, errors.Compose());
        }

        [Fact]
        public void Test_Read_Should_Not_Read_If_Could_Not_Retrieve_Server_Url_Plain_Nested()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "a-project-name",
                [GitlabKeys.WebUrl] = "a-web-url",
                [GitlabKeys.PathWithNamespace] = "a-very-long-path"
            };
            var request = new JObject
            {
                ["project"] = data
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.Read(request["project"], errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);

            const string expectedError = "1. Can not retrieve the server url out of \"project.web_url\"";
            Assert.Equal(expectedError, errors.Compose());
        }

        [Fact]
        public void Test_Read_Should_Read_ProjectInfo()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "a-project-name",
                [GitlabKeys.WebUrl] = "a-web-url/namespace",
                [GitlabKeys.PathWithNamespace] = "/namespace"
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.Read(data, errors);

            Assert.Equal("a-project-name", info.Name);
            Assert.Equal("a-web-url/namespace", info.Url);
            Assert.Equal("a-web-url", info.Server);
        }
    }
}
