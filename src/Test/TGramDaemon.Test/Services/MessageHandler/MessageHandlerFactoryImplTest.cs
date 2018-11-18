using System;
using Autofac.Extras.Moq;
using Moq;
using TGramDaemon.Services.MessageHandler;
using Xunit;

namespace TGramDaemon.Test.Services.MessageHandler
{
    public class MessageHandlerFactoryImplTest
    {
        [Fact]
        public void Test_CreateHandler_Should_Create_A_New_Handler()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockHandler = new Mock<IMessageHandler>();

                mock.Mock<IServiceProvider>()
                    .Setup(p => p.GetService(typeof(IMessageHandler)))
                    .Returns(mockHandler.Object);

                var factory = mock.Create<MessageHandlerFactoryImpl>();
                IMessageHandler handler = factory.CreateHandler();

                Assert.Same(mockHandler.Object, handler);

                mock.Mock<IServiceProvider>()
                    .Verify(p => p.GetService(typeof(IMessageHandler)), Times.Once);
            }
        }
    }
}
