#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

public class FrontEndServiceTests
{
    private const string HttpClientName = "PublicApp";

    [Fact]
    public async Task CreateUniversalTable_ClientReturnsOk()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.OK);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateUniversalTable(
            new TableBuilderResultViewModel(),
            new TableBuilderConfiguration());

        MockUtils.VerifyAllMocks(httpMessageHandler, httpClientFactory);

        result.AssertRight();
    }

    [Fact]
    public async Task CreateUniversalTable_ClientReturnsNotFound()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.NotFound);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateUniversalTable(
            new TableBuilderResultViewModel(),
            new TableBuilderConfiguration());

        MockUtils.VerifyAllMocks(httpMessageHandler, httpClientFactory);

        result.AssertNotFound();
    }

    [Fact]
    public async Task CreateUniversalTable_ClientReturnsError()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.BadRequest);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateUniversalTable(
            new TableBuilderResultViewModel(),
            new TableBuilderConfiguration());

        MockUtils.VerifyAllMocks(httpMessageHandler, httpClientFactory);

        result.AssertInternalServerError();
    }

    private static Mock<HttpMessageHandler> SetupFrontendHttpMessageHandler(HttpStatusCode httpStatusCode)
    {
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = httpStatusCode,
            Content = new JsonNetContent(
                content: new
                {
                    message = "",
                    status = httpStatusCode
                })
        };

        var httpMessageHandler = HttpClientTestUtils.CreateHttpMessageHandlerMock(
            httpMethod: HttpMethod.Post,
            requestUri: "api/permalink",
            httpResponseMessage: httpResponseMessage);

        return httpMessageHandler;
    }

    private static FrontendService BuildService(
        IHttpClientFactory? httpClientFactory = null)
    {
        return new FrontendService(
            logger: Mock.Of<ILogger<FrontendService>>(),
            httpClientFactory: httpClientFactory ?? Mock.Of<IHttpClientFactory>()
        );
    }
}
