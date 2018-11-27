using System;
using Autofac;
using Autofac.Extras.Moq;
using Moq;
using Newtonsoft.Json.Linq;
using TGramTestUtil;
using TGramWeb.Services.GitlabProcessService;
using Xunit;
using Xunit.Abstractions;

namespace TGramWeb.Test.Services.GitlabProcessService
{
    public class GitlabProcessServiceImplTest
    {
        private readonly ITestOutputHelper output;

        public GitlabProcessServiceImplTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_ProcessRequest_Throws_If_Called_With_Illegal_Args()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var service = mock.Create<GitlabProcessServiceImpl>();
                var ex = Assert.Throws<ArgumentNullException>(() => service.ProcessRequest(null));
                Assert.Contains("request", ex.Message);
            }
        }

        [Fact]
        public void Test_ProcessRequest_Returns_The_First_Successful_Result()
        {
            var m1 = new Mock<IGitlabProcessor>();
            m1.Setup(p => p.Process(It.IsAny<JObject>()))
              .Returns(RequestProcessResult.CreateNoResult());//no result
            var m2 = new Mock<IGitlabProcessor>();
            m2.Setup(p => p.Process(It.IsAny<JObject>()))
              .Returns(RequestProcessResult.CreateSuccess());//success
            var m3 = new Mock<IGitlabProcessor>();
            m3.Setup(p => p.Process(It.IsAny<JObject>()))
              .Returns(RequestProcessResult.CreateNoResult());//should not have been called

            using (AutoMock mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterXUnit(this.output);
                builder.RegisterInstance(m1.Object);
                builder.RegisterInstance(m2.Object);
                builder.RegisterInstance(m3.Object);
            }))
            {
                var service = mock.Create<GitlabProcessServiceImpl>();

                JObject request = JObject.Parse("{prop:'value'}");
                RequestProcessResult result = service.ProcessRequest(request);

                Assert.True(result.Success);

                m1.Verify(p => p.Process(request), Times.Once);
                m2.Verify(p => p.Process(request), Times.Once);
                m3.Verify(p => p.Process(request), Times.Never);
            }
        }

        [Fact]
        public void Test_ProcessRequest_Returns_Failure_If_No_Success_Result_Received()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                mock.Mock<IGitlabProcessor>()
                    .Setup(p => p.Process(It.IsAny<JObject>()))
                    .Returns(RequestProcessResult.CreateNoResult());

                var service = mock.Create<GitlabProcessServiceImpl>();

                JObject request = JObject.Parse("{prop:'value'}");
                RequestProcessResult result = service.ProcessRequest(request);

                Assert.False(result.Success);
                Assert.Equal("The system is not capable of processing such requests", result.Reason);
            }
        }
    }
}