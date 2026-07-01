#nullable enable
using System.Net;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using Moq;
using RichardSzalay.MockHttp;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Screener;

public class DataSetScreenerClientTests
{
    private static readonly Uri BaseUri = new("http://localhost/api");
    private readonly MockHttpMessageHandler _mockHttp;

    protected DataSetScreenerClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
    }

    public class AuthenticationTests : DataSetScreenerClientTests
    {
        [Fact]
        public async Task AuthenticationManagerCalled()
        {
            Guid[] dataSetIds = [Guid.NewGuid()];

            _mockHttp
                .Expect(HttpMethod.Get, $"{BaseUri.AbsoluteUri}/progress?data_set_id={dataSetIds[0]}")
                .Respond(
                    HttpStatusCode.OK,
                    "application/json",
                    JsonSerializer.Serialize(Array.Empty<DataSetScreenerProgressResponse>())
                );

            var authenticationManager = new Mock<IHttpClientAzureAuthenticationManager<DataScreenerOptions>>(
                MockBehavior.Strict
            );

            authenticationManager
                .Setup(m => m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var dataSetScreenerClient = BuildService(azureAuthenticationManager: authenticationManager.Object);

            await dataSetScreenerClient.GetScreenerProgress(dataSetIds, CancellationToken.None);

            authenticationManager.Verify(
                m => m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }

    public class GetScreeningProgressTests : DataSetScreenerClientTests
    {
        [Fact]
        public async Task Success()
        {
            Guid[] dataSetIds = [Guid.NewGuid(), Guid.NewGuid()];

            DataSetScreenerProgressResponse[] responseBody =
            [
                new()
                {
                    DataSetId = dataSetIds[0],
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = 50.12,
                        Completed = false,
                        Passed = false,
                        Stage = "Validation",
                    },
                },
                new()
                {
                    DataSetId = dataSetIds[1],
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = 100.00,
                        Completed = true,
                        Passed = true,
                        Stage = "Screening",
                    },
                },
            ];

            _mockHttp
                .Expect(HttpMethod.Get, $"{BaseUri.AbsoluteUri}/progress")
                .WithQueryString($"data_set_id={dataSetIds[0]}&data_set_id={dataSetIds[1]}")
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(responseBody));

            var dataSetScreenerClient = BuildService();

            var response = await dataSetScreenerClient.GetScreenerProgress(
                dataSetIds: dataSetIds,
                CancellationToken.None
            );

            _mockHttp.VerifyNoOutstandingExpectation();

            Assert.Equivalent(responseBody, response);
        }
    }

    public class GetScreeningCompletionReportsTests : DataSetScreenerClientTests
    {
        [Fact]
        public async Task Success()
        {
            Guid[] dataSetIds = [Guid.NewGuid(), Guid.NewGuid()];

            DataSetScreenerCompletionReportResponse[] responseBody =
            [
                new()
                {
                    DataSetId = dataSetIds[0],
                    CompletionReport = new DataSetScreenerResponse
                    {
                        OverallResult = "passed",
                        Passed = true,
                        PublicApiCompatible = true,
                        TestResults = [],
                    },
                },
                new()
                {
                    DataSetId = dataSetIds[1],
                    CompletionReport = new DataSetScreenerResponse
                    {
                        OverallResult = "passed",
                        Passed = true,
                        PublicApiCompatible = true,
                        TestResults = [new DataScreenerTestResult { Stage = "stage 1", TestFunctionName = "test 1" }],
                    },
                },
            ];

            _mockHttp
                .Expect(HttpMethod.Get, $"{BaseUri.AbsoluteUri}/completion-reports")
                .WithQueryString($"data_set_id={dataSetIds[0]}&data_set_id={dataSetIds[1]}")
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(responseBody));

            var dataSetScreenerClient = BuildService();

            var response = await dataSetScreenerClient.GetScreenerCompletionReports(
                dataSetIds: dataSetIds,
                CancellationToken.None
            );

            _mockHttp.VerifyNoOutstandingExpectation();

            Assert.Equivalent(responseBody, response);
        }
    }

    public class DeleteScreenerProgressAndCompletionFiles : DataSetScreenerClientTests
    {
        [Fact]
        public async Task Success()
        {
            Guid[] dataSetIds = [Guid.NewGuid(), Guid.NewGuid()];

            _mockHttp
                .Expect(HttpMethod.Delete, $"{BaseUri.AbsoluteUri}/progress-and-completion-files")
                .WithQueryString($"data_set_id={dataSetIds[0]}&data_set_id={dataSetIds[1]}")
                .WithContent("{}")
                .Respond(HttpStatusCode.NoContent);

            var dataSetScreenerClient = BuildService();

            await dataSetScreenerClient.DeleteScreenerProgressAndCompletionFiles(
                dataSetIds: dataSetIds,
                CancellationToken.None
            );

            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }

    private DataSetScreenerClient BuildService(
        IHttpClientAzureAuthenticationManager<DataScreenerOptions>? azureAuthenticationManager = null
    )
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = BaseUri;

        var authenticationManager =
            azureAuthenticationManager
            ?? Mock.Of<IHttpClientAzureAuthenticationManager<DataScreenerOptions>>(MockBehavior.Loose);

        return new DataSetScreenerClient(client, authenticationManager);
    }
}
