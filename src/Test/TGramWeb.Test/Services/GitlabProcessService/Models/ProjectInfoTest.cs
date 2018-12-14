using System;
using Newtonsoft.Json.Linq;
using TGramWeb.Services.GitlabProcessService;
using TGramWeb.Services.GitlabProcessService.Models;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService.Models
{
    public class ProjectInfoTest
    {
        [Fact]
        public void Test_TryRead_Should_Throw_If_Called_With_Illegal_Args()
        {
            void Caller() => ProjectInfo.TryRead(new JObject(), null);
            var ex = Assert.Throws<ArgumentNullException>((Action) Caller);
            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Test_TryRead_Returns_Null_If_Project_Is_Null()
        {
            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.TryRead(null, errors);

            Assert.Null(info);
            Assert.False(errors.HasAny);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("project-name", null, null)]
        [InlineData(null, "project-url", null)]
        [InlineData(null, null, "path")]
        public void Test_TryRead_Should_Not_Read_If_Project_Lacks_Data(string projectName, string projectUrl, string path)
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = projectName,
                [GitlabKeys.WebUrl] = projectUrl,
                [GitlabKeys.PathWithNamespace] = path
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.TryRead(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);
        }

        [Fact]
        public void Test_TryRead_Should_Not_Read_If_Could_Not_Retrieve_Server_Url_Plain()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "a-project-name",
                [GitlabKeys.WebUrl] = "a-web-url",
                [GitlabKeys.PathWithNamespace] = "a-very-long-path"
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.TryRead(data, errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);

            const string expectedError = "1. Can not retrieve the server url out of \"web_url\"";
            Assert.Equal(expectedError, errors.Compose());
        }

        [Fact]
        public void Test_TryRead_Should_Not_Read_If_Could_Not_Retrieve_Server_Url_Plain_Nested()
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
            ProjectInfo info = ProjectInfo.TryRead(request["project"], errors);

            Assert.Null(info);
            Assert.True(errors.HasAny);

            const string expectedError = "1. Can not retrieve the server url out of \"project.web_url\"";
            Assert.Equal(expectedError, errors.Compose());
        }

        [Fact]
        public void Test_TryRead_Should_Read_ProjectInfo()
        {
            var data = new JObject
            {
                [GitlabKeys.Name] = "a-project-name",
                [GitlabKeys.WebUrl] = "a-web-url/namespace",
                [GitlabKeys.PathWithNamespace] = "/namespace"
            };

            var errors = new JTokenErrors();
            ProjectInfo info = ProjectInfo.TryRead(data, errors);

            Assert.Equal("a-project-name", info.Name);
            Assert.Equal("a-web-url/namespace", info.Url);
            Assert.Equal("a-web-url", info.Server);
        }
    }
}
