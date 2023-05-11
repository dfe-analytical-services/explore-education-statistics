#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

public class FrontEndServiceTests
{
    private const string HttpClientName = "PublicApp";

    private const string TableJson = @"
    {
      ""thead"": [
        [
          { ""colSpan"": 1, ""rowSpan"": 1, ""tag"": ""td"" },
          {
            ""colSpan"": 1,
            ""rowSpan"": 1,
            ""scope"": ""col"",
            ""text"": ""2022"",
            ""tag"": ""th""
          },
          {
            ""colSpan"": 1,
            ""rowSpan"": 1,
            ""scope"": ""col"",
            ""text"": ""2023"",
            ""tag"": ""th""
          }
        ]
      ],
      ""tbody"": [
        [
          {
            ""rowSpan"": 1,
            ""colSpan"": 1,
            ""scope"": ""row"",
            ""text"": ""Admission Numbers"",
            ""tag"": ""th""
          },
          { ""tag"": ""td"", ""text"": ""7,731"" },
          { ""tag"": ""td"", ""text"": ""7,357"" }
        ]
      ]
    }";

    [Fact]
    public async Task CreateTable_ClientReturnsOk()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.OK);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateTable(
            new TableBuilderResultViewModel(),
            new TableBuilderConfiguration());

        MockUtils.VerifyAllMocks(httpMessageHandler, httpClientFactory);

        var viewModel = result.AssertRight();

        Assert.Equal("Admission Numbers for 'Sample publication' in North East between 2022 and 2023",
            viewModel.Caption);
        Assert.Equal(JObject.Parse(TableJson), viewModel.Json);
    }

    [Fact]
    public async Task CreateTable_ClientReturnsNotFound()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.NotFound);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateTable(
            new TableBuilderResultViewModel(),
            new TableBuilderConfiguration());

        MockUtils.VerifyAllMocks(httpMessageHandler, httpClientFactory);

        result.AssertNotFound();
    }

    [Fact]
    public async Task CreateTable_ClientReturnsError()
    {
        var httpMessageHandler = SetupFrontendHttpMessageHandler(HttpStatusCode.BadRequest);
        var httpClientFactory = HttpClientTestUtils.CreateHttpClientFactoryMock(
            clientName: HttpClientName,
            httpMessageHandler: httpMessageHandler.Object);

        var service = BuildService(
            httpClientFactory: httpClientFactory.Object);

        var result = await service.CreateTable(
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
            Content = new StringContent(@$"
            {{
              ""caption"": ""Admission Numbers for 'Sample publication' in North East between 2022 and 2023"",
              ""json"": {TableJson}
            }}")
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
