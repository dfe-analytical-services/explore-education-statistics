#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetScreenerClientTests
{
    private static readonly Uri BaseUri = new("http://localhost/api/screen");
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly JsonSerializerOptions _requestSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // API is case sensitive
    };

    protected DataSetScreenerClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
    }

    public class AuthenticationTests : DataSetScreenerClientTests
    {
        [Fact(Skip = "EES-6341: Skip until screener has been re-enabled")]
        public async Task AuthenticationManagerCalled()
        {
            var responseBody = new DataSetScreenerResponse
            {
                Message = "",
                OverallResult = ScreenerResult.Failed,
                TestResults = []
            };

            _mockHttp.Expect(HttpMethod.Post, BaseUri.AbsoluteUri)
                .Respond(HttpStatusCode.Accepted, "application/json", JsonSerializer.Serialize(responseBody));

            var authenticationManager =
                new Mock<IHttpClientAzureAuthenticationManager<DataScreenerClientOptions>>(MockBehavior.Strict);

            authenticationManager
                .Setup(m =>
                    m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var dataSetScreenerClient = BuildService(
                azureAuthenticationManager: authenticationManager.Object);

            await dataSetScreenerClient.ScreenDataSet(new DataSetScreenerRequest
            {
                DataFileName = "",
                DataFilePath = "",
                MetaFileName = "",
                MetaFilePath = "",
                StorageContainerName = ""
            }, default);

            authenticationManager.Verify(m =>
                m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class ScreenDataSetTests : DataSetScreenerClientTests
    {
        [Fact(Skip = "EES-6341: Skip until screener has been re-enabled")]
        public async Task Success()
        {
            var request = new DataSetScreenerRequest
            {
                DataFileName = "data-file-name",
                DataFilePath = "data-file-path",
                MetaFileName = "meta-file-name",
                MetaFilePath = "meta-file-path",
                StorageContainerName = "storage-container-name"
            };

            var responseBody = new DataSetScreenerResponse
            {
                Message = "A message",
                OverallResult = ScreenerResult.Failed,
                TestResults = [
                    new DataScreenerTestResult
                    {
                        Stage = Stage.PreScreening1,
                        Result = TestResult.WARNING,
                        Notes = "Some notes",
                        TestFunctionName = "Test 1"
                    },
                    new DataScreenerTestResult
                    {
                        Stage = Stage.Passed,
                        Result = TestResult.PASS,
                        TestFunctionName = "Test 2"
                    }
                ]
            };

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri.AbsoluteUri)
                .WithJsonContent(request, _requestSerializerOptions)
                .Respond(HttpStatusCode.Accepted, "application/json", JsonSerializer.Serialize(responseBody));

            var dataSetScreenerClient = BuildService();

            var response = await dataSetScreenerClient.ScreenDataSet(request, default);

            _mockHttp.VerifyNoOutstandingExpectation();

            Assert.Equivalent(responseBody, response);
        }
    }

    private DataSetScreenerClient BuildService(
        IHttpClientAzureAuthenticationManager<DataScreenerClientOptions>? azureAuthenticationManager = null)
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = BaseUri;

        var authenticationManager = azureAuthenticationManager ??
                                    Mock.Of<IHttpClientAzureAuthenticationManager<DataScreenerClientOptions>>(
                                        MockBehavior.Loose);

        return new DataSetScreenerClient(
            client,
            authenticationManager);
    }
}
