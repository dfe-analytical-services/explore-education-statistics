using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public abstract class ContentApiClientTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _client;
    private readonly ContentApiClient _contentApiClient;

    public ContentApiClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _client = _mockHttp.ToHttpClient();
        _client.BaseAddress = new Uri("http://localhost/");
        _contentApiClient = new ContentApiClient(Mock.Of<ILogger<ContentApiClient>>(), _client);
    }

    public class ListPublicationsTests : ContentApiClientTests
    {
        [Fact]
        public async Task HttpClientBadRequest_ReturnsBadRequest()
        {
            _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
                .Respond(HttpStatusCode.BadRequest, new StringContent("test message"));

            var response = await _contentApiClient.ListPublications(
                page: It.IsAny<int>(),
                pageSize: It.IsAny<int>(),
                search: It.IsAny<string>(),
                publicationIds: It.IsAny<IEnumerable<Guid>>());

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            Assert.IsType<BadRequestObjectResult>(left);
        }

        [Theory]
        [InlineData(HttpStatusCode.RequestTimeout)]
        public async Task HttpClientRespondsWithUnsuccessfulStatusCode_ThrowsHttpRequestExceptionAndLogsError(
                HttpStatusCode responseStatusCode)
        {
            _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
                .Respond(responseStatusCode, new StringContent("test message"));

            var action = async () => await _contentApiClient.ListPublications(
                page: It.IsAny<int>(),
                pageSize: It.IsAny<int>(),
                search: It.IsAny<string>(),
                publicationIds: It.IsAny<IEnumerable<Guid>>());

            await Assert.ThrowsAsync<HttpRequestException>(action);

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task HttpClientRespondsWithSuccess_ReturnsPublications()
        {
            var results = new List<PublicationSearchResultViewModel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                }
            };
            var responseBody =
                new PaginatedListViewModel<PublicationSearchResultViewModel>(results, results.Count, 1, results.Count);

            _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(responseBody));

            var response = await _contentApiClient.ListPublications(
                page: It.IsAny<int>(),
                pageSize: It.IsAny<int>(),
                search: It.IsAny<string>(),
                publicationIds: It.IsAny<IEnumerable<Guid>>());

            _mockHttp.VerifyNoOutstandingExpectation();

            var right = response.AssertRight();
            var publication = Assert.Single(right.Results);
            Assert.Equal(results.Single().Id, publication.Id);
        }
    }
}
