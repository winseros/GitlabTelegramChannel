using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;
using TGramDaemon.Services.TelegramService;
using TGramTestUtil;
using Xunit;
using Xunit.Abstractions;
using HttpClientFactoryExtensions = TGramDaemon.Services.TelegramService.HttpClientFactoryExtensions;

namespace TGramDaemon.Test.Services.TelegramService
{
    public class TelegramServiceImplTest
    {
        public class SendMessageAsync
        {
            private readonly ITestOutputHelper xUnitOutput;

            public SendMessageAsync(ITestOutputHelper xUnitOutput)
            {
                this.xUnitOutput = xUnitOutput;
            }

            [Fact]
            public async Task It_Should_Throw_If_Called_With_Illegal_Args()
            {
                using (AutoMock mock = AutoMock.GetLoose())
                {
                    IOptions<TelegramOptions> options = Options.Create(new TelegramOptions {Endpoint = new Uri("https://api.telegram.org"), Channel = "abc", Token = "cde"});
                    mock.Provide(options);

                    var service = mock.Create<TelegramServiceImpl>();
                    async Task Caller() => await service.SendMessageAsync(null, CancellationToken.None);
                    ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(Caller);
                    Assert.Contains("message", ex.Message);
                }
            }

            [Fact]
            public async Task It_Should_Send_Message_To_The_Endpoint()
            {
                using (AutoMock mock = AutoMock.GetLoose(this.xUnitOutput.Capture()))
                {
                    IOptions<TelegramOptions> options = Options.Create(new TelegramOptions {Endpoint = new Uri("https://api.telegram.org"), Channel = "abc", Token = "cde"});
                    mock.Provide(options);

                    var response = new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent("some-response")};
                    var mockMessageHandler = new MockMessageHandler(response);
                    mock.Mock<IHttpClientFactory>()
                        .Setup(p => p.CreateClient(HttpClientFactoryExtensions.ClientName))
                        .Returns(new HttpClient(mockMessageHandler) {BaseAddress = options.Value.Endpoint});

                    var service = mock.Create<TelegramServiceImpl>();
                    await service.SendMessageAsync("some-message", CancellationToken.None);

                    mock.Mock<IHttpClientFactory>()
                        .Verify(p => p.CreateClient(HttpClientFactoryExtensions.ClientName), Times.Once);

                    HttpRequestMessage request = mockMessageHandler.MessageSent;
                    Assert.Equal(HttpMethod.Post, request.Method);
                    // ReSharper disable once StringLiteralTypo
                    Assert.Equal("/botcde/sendMessage", request.RequestUri.AbsolutePath);

                    const string expectedContent = "{\"chat_id\":\"abc\",\"text\":\"some-message\",\"parse_mode\":\"Markdown\"}";
                    string content = await request.Content.ReadAsStringAsync();
                    Assert.Equal(expectedContent, content);
                }
            }
        }

        private class MockMessageHandler: HttpMessageHandler
        {
            private readonly HttpResponseMessage mockResponse;

            public MockMessageHandler(HttpResponseMessage mockResponse)
            {
                this.mockResponse = mockResponse;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                this.MessageSent = request;
                return Task.FromResult(this.mockResponse);
            }

            public HttpRequestMessage MessageSent { get; private set; }
        }
    }
}