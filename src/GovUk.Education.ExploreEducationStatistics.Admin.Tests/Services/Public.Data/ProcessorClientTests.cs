using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Public.Data;

public class ProcessorClientTests
{
    private static readonly Uri BaseUri = new("http://localhost");
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly ProcessorClient _processorClient;

    public ProcessorClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = BaseUri;
        _processorClient = new ProcessorClient(Mock.Of<ILogger<ProcessorClient>>(), client);
    }

    public class ProcessTests : ProcessorClientTests
    {
        private static readonly Uri Uri = new(BaseUri, "api/orchestrators/processor");

        [Fact]
        public async Task HttpClientAccepted_ReturnsPublications()
        {
            var responseBody = new ProcessorTriggerResponseViewModel
            {
                DataSetVersionId = Guid.NewGuid(),
                InstanceId = Guid.NewGuid()
            };

            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(HttpStatusCode.Accepted, "application/json", JsonConvert.SerializeObject(responseBody));

            var response = await _processorClient.Process(releaseFileId: It.IsAny<Guid>());

            _mockHttp.VerifyNoOutstandingExpectation();

            var right = response.AssertRight();
            Assert.Equal(responseBody.DataSetVersionId, right.DataSetVersionId);
            Assert.Equal(responseBody.InstanceId, right.InstanceId);
        }

        [Theory]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.Gone)]
        [InlineData(HttpStatusCode.NotAcceptable)]
        [InlineData(HttpStatusCode.BadRequest)]
        public async Task HttpClientFailureStatusCode_ThrowsException(
            HttpStatusCode responseStatusCode)
        {
            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(responseStatusCode);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _processorClient.Process(releaseFileId: It.IsAny<Guid>());
            });

            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
