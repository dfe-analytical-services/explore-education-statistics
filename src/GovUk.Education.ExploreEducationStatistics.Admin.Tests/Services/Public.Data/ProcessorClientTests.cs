#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

    public class CreateInitialDataSetVersionTests : ProcessorClientTests
    {
        private static readonly Uri Uri = new(BaseUri, "api/CreateInitialDataSetVersion");

        [Fact]
        public async Task HttpClientSuccess()
        {
            var responseBody = new CreateInitialDataSetVersionResponseViewModel
            {
                DataSetId = Guid.NewGuid(),
                DataSetVersionId = Guid.NewGuid(),
                InstanceId = Guid.NewGuid()
            };

            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(HttpStatusCode.Accepted, "application/json", JsonConvert.SerializeObject(responseBody));

            var response = await _processorClient.CreateInitialDataSetVersion(releaseFileId: Guid.NewGuid());

            _mockHttp.VerifyNoOutstandingExpectation();

            var right = response.AssertRight();
            Assert.Equal(responseBody.DataSetId, right.DataSetId);
            Assert.Equal(responseBody.DataSetVersionId, right.DataSetVersionId);
            Assert.Equal(responseBody.InstanceId, right.InstanceId);
        }

        [Fact]
        public async Task HttpClientBadRequest_ReturnsBadRequest()
        {
            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(
                    HttpStatusCode.BadRequest,
                    JsonContent.Create(new ValidationProblemViewModel
                    {
                        Errors = new ErrorViewModel[]
                        {
                            new() {
                               Code = Errors.Error1.ToString()
                            }
                        }
                    }));

            var response = await _processorClient.CreateInitialDataSetVersion(releaseFileId: Guid.NewGuid());

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            left.AssertValidationProblem(Errors.Error1);
        }

        [Fact]
        public async Task HttpClientNotFound_ReturnsNotFound()
        {
            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(HttpStatusCode.NotFound);

            var response = await _processorClient.CreateInitialDataSetVersion(releaseFileId: Guid.NewGuid());

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            left.AssertNotFoundResult();
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
        public async Task HttpClientFailureStatusCode_ThrowsException(
            HttpStatusCode responseStatusCode)
        {
            _mockHttp.Expect(HttpMethod.Post, Uri.AbsoluteUri)
                .Respond(responseStatusCode);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _processorClient.CreateInitialDataSetVersion(releaseFileId: Guid.NewGuid());
            });

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        private enum Errors
        {
            Error1,
        }
    }
}
