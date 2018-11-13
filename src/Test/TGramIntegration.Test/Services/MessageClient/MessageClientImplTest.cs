using System;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using TGramIntegration.Services.MessageClient;
using Xunit;

namespace TGramIntegration.Test.Services.MessageClient
{
    public class MessageClientImplTest
    {
        public class ScheduleDelivery
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void It_Should_Throw_If_Called_With_Illegal_Args(string value)
            {
                using (AutoMock mock = AutoMock.GetLoose())
                {
                    mock.Provide<IOptions<MessageClientOptions>>(new OptionsWrapper<MessageClientOptions>(new MessageClientOptions
                    {
                        Address = "inproc://address"
                    }));

                    var service = mock.Create<MessageClientImpl>();
                    var ex = Assert.Throws<ArgumentNullException>(() => service.ScheduleDelivery(value));
                    Assert.Contains("message", ex.Message);
                }
            }

            [Fact]
            public void It_Should_Send_Message_To_Socket()
            {
                const string address = "inproc://address";
                using (var socket = new PullSocket(address))
                using (AutoMock mock = AutoMock.GetLoose())
                {
                    mock.Provide<IOptions<MessageClientOptions>>(new OptionsWrapper<MessageClientOptions>(new MessageClientOptions
                    {
                        Address = address
                    }));

                    var service = mock.Create<MessageClientImpl>();
                    service.ScheduleDelivery("A some message 1");
                    service.ScheduleDelivery("A some message 2");
                    service.ScheduleDelivery("A some message 3");

                    Assert.Equal("A some message 1", socket.ReceiveFrameString());
                    Assert.Equal("A some message 2", socket.ReceiveFrameString());
                    Assert.Equal("A some message 3", socket.ReceiveFrameString());
                }
            }
        }
    }
}
