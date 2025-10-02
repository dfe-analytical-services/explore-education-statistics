#nullable enable
using System.Net;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Authentication;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Public.Data;

public class PublicDataApiClientTests
{
    private static readonly Uri BaseUri = new("http://localhost");
    private readonly MockHttpMessageHandler _mockHttp;

    protected PublicDataApiClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
    }

    public class AuthenticationTests : PublicDataApiClientTests
    {
        [Fact]
        public async Task AuthenticationManagerCalled()
        {
            var responseMessage = new HttpResponseMessage();

            var dataSetId = Guid.NewGuid();
            var dataSetVersion = "1.1";

            var uri = new Uri(BaseUri, $"v1/data-sets/{dataSetId}/versions/{dataSetVersion}/changes");

            _mockHttp
                .Expect(HttpMethod.Get, uri.AbsoluteUri)
                .Respond(HttpStatusCode.Accepted, "application/json", JsonConvert.SerializeObject(responseMessage));

            var authenticationManager = new Mock<IHttpClientAzureAuthenticationManager<PublicDataApiOptions>>(
                MockBehavior.Strict
            );

            authenticationManager
                .Setup(m => m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var publicDataApiClient = BuildService(azureAuthenticationManager: authenticationManager.Object);

            await publicDataApiClient.GetDataSetVersionChanges(dataSetId: dataSetId, dataSetVersion: dataSetVersion);

            authenticationManager.Verify(
                m => m.AddAuthentication(It.IsAny<HttpClient>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }

    public class GetDataSetVersionChangesTests : PublicDataApiClientTests
    {
        [Fact]
        public async Task HttpClientSuccess()
        {
            var dataSetId = Guid.NewGuid();
            var dataSetVersion = "1.1";

            var uri = new Uri(BaseUri, $"v1/data-sets/{dataSetId}/versions/{dataSetVersion}/changes");

            _mockHttp
                .Expect(HttpMethod.Get, uri.AbsoluteUri)
                .Respond(HttpStatusCode.OK, "application/json", "Response text");

            var publicDataApiClient = BuildService();

            var response = await publicDataApiClient.GetDataSetVersionChanges(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion
            );

            _mockHttp.VerifyNoOutstandingExpectation();

            var result = response.AssertRight();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Response text", await result.Content.ReadAsStringAsync());
        }
    }

    private PublicDataApiClient BuildService(
        IHttpClientAzureAuthenticationManager<PublicDataApiOptions>? azureAuthenticationManager = null
    )
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = BaseUri;

        var authenticationManager =
            azureAuthenticationManager
            ?? Mock.Of<IHttpClientAzureAuthenticationManager<PublicDataApiOptions>>(MockBehavior.Loose);
        return new PublicDataApiClient(Mock.Of<ILogger<PublicDataApiClient>>(), client, authenticationManager);
    }
}
