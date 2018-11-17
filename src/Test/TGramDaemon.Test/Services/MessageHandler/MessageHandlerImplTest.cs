using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using TGramDaemon.Services.MessageHandler;
using TGramTestUtil;
using Xunit;
using Xunit.Abstractions;

namespace TGramDaemon.Test.Services.MessageHandler
{
    public class MessageHandlerImplTest
    {
        public class Start
        {
            private readonly ITestOutputHelper output;

            public Start(ITestOutputHelper output)
            {
                this.output = output;
            }

            [Fact]
            public void It_Should_Throw_If_Called_Twice()
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
            public void It_Should_Process_Incoming_Messages()
            {
                using (AutoMock mock = AutoMock.GetLoose(this.output.Capture()))
                {
                    IOptions<MessageHandlerOptions> options = Options.Create(new MessageHandlerOptions {Address = "tcp://127.0.0.1:51861"});
                    mock.Provide(options);

                    using (var handler = mock.Create<MessageHandlerImpl>())
                    {
                        handler.Start();
                        //--
                    }
                }
            }
        }

        public class StopAsync
        {
            private readonly ITestOutputHelper output;

            public StopAsync(ITestOutputHelper output)
            {
                this.output = output;
            }

            [Fact]
            public async Task It_Should_Throw_If_Called_Twice()
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
}
