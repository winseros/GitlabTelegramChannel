using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;
using NetMQ;
using NetMQ.Sockets;
using TGramDaemon.Services.MessageHandler;
using TGramDaemon.Services.TelegramService;
using TGramTestUtil;
using Xunit;
using Xunit.Abstractions;

namespace TGramDaemon.Test.Services.MessageHandler
{
    public class MessageHandlerImplTest
    {
        private readonly ITestOutputHelper output;

        public MessageHandlerImplTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_Start_Should_Throw_If_Called_Twice()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                IOptions<MessageHandlerOptions> options = Options.Create(new MessageHandlerOptions {Address = "tcp://127.0.0.1:51861"});
                mock.Provide(options);

                using (var handler = mock.Create<MessageHandlerImpl>())
                {
                    handler.Start();
                    var ex = Assert.Throws<InvalidOperationException>(() => handler.Start());
                    Assert.Equal("The handler has already been started", ex.Message);
                }
            }
        }

        [Fact]
        public void Test_Start_Should_Process_Incoming_Messages()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                IOptions<MessageHandlerOptions> options = Options.Create(new MessageHandlerOptions {Address = "tcp://127.0.0.1:51861"});
                mock.Provide(options);

                using (var pushSocket = new PushSocket($"@{options.Value.Address}"))
                using (var handler = mock.Create<MessageHandlerImpl>())
                {
                    handler.Start();

                    pushSocket.SendFrame("some-message");

                    Mock<ITelegramService> telegramService = mock.Mock<ITelegramService>();
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    Predicate<CancellationToken> isNotCancelled = token => !token.IsCancellationRequested;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            telegramService.Verify(p => p.SendMessageAsync("some-message", Match.Create(isNotCancelled)), Times.Once);
                            break;
                        }
                        catch (MockException)
                        {
                            if (cts.Token.IsCancellationRequested)
                                throw;
                        }
                    }
                }
            }
        }

        [Fact]
        public void Test_Start_Should_Not_Fail_In_Case_Of_Processing_Exception()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                IOptions<MessageHandlerOptions> options = Options.Create(new MessageHandlerOptions {Address = "tcp://127.0.0.1:51861"});
                mock.Provide(options);

                mock.Mock<ITelegramService>()
                    .SetupSequence(service => service.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Throws(new Exception("Something threw an exception"))
                    .Returns(Task.CompletedTask);

                using (var pushSocket = new PushSocket($"@{options.Value.Address}"))
                using (var handler = mock.Create<MessageHandlerImpl>())
                {
                    handler.Start();

                    pushSocket.SendFrame("some-message1");
                    pushSocket.SendFrame("some-message2");

                    Mock<ITelegramService> telegramService = mock.Mock<ITelegramService>();
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    Predicate<CancellationToken> isNotCancelled = token => !token.IsCancellationRequested;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            telegramService.Verify(p => p.SendMessageAsync("some-message1", Match.Create(isNotCancelled)), Times.Once);
                            telegramService.Verify(p => p.SendMessageAsync("some-message2", Match.Create(isNotCancelled)), Times.Once);
                            break;
                        }
                        catch (MockException)
                        {
                            if (cts.Token.IsCancellationRequested)
                                throw;
                        }
                    }
                }
            }
        }




        [Fact]
        public async Task Test_StopAsync_Should_Throw_If_Called_Twice()
        {
            using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
            {
                IOptions<MessageHandlerOptions> options = Options.Create(new MessageHandlerOptions {Address = "tcp://127.0.0.1:51861"});
                mock.Provide(options);

                using (var handler = mock.Create<MessageHandlerImpl>())
                {
                    handler.Start();
                    await handler.StopAsync(CancellationToken.None);
                    InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.StopAsync(CancellationToken.None));
                    Assert.Equal("The handler has not been started", ex.Message);
                }
            }
        }
    }
}
