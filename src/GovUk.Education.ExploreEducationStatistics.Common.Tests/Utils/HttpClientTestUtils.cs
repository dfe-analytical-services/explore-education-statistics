#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class HttpClientTestUtils
{
    private const string BaseAddress = "http://test.localhost";

    public static Mock<IHttpClientFactory> CreateHttpClientFactoryMock(string clientName,
        HttpMessageHandler httpMessageHandler)
    {
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory.Setup(factory => factory.CreateClient(clientName))
            .Returns(httpClient);

        return httpClientFactory;
    }

    public static Mock<HttpMessageHandler> CreateHttpMessageHandlerMock(HttpMethod httpMethod,
        string requestUri,
        HttpResponseMessage httpResponseMessage)
    {
        var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var expectedRequestUri = new Uri($"{BaseAddress}/{requestUri}");

        messageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.Method == httpMethod
                                                         && message.RequestUri == expectedRequestUri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        return messageHandlerMock;
    }
}
