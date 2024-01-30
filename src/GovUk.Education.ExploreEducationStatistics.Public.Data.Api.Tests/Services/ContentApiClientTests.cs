using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
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

            var response = await _contentApiClient.ListPublications(
                page: It.IsAny<int>(),
                pageSize: It.IsAny<int>(),
                search: It.IsAny<string>(),
                publicationIds: It.IsAny<IEnumerable<Guid>>());

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            left.AssertValidationProblem(Errors.Error1);
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
            _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
                .Respond(responseStatusCode);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _contentApiClient.ListPublications(
                    page: It.IsAny<int>(),
                    pageSize: It.IsAny<int>(),
                    search: It.IsAny<string>(),
                    publicationIds: It.IsAny<IEnumerable<Guid>>());
            });

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task HttpClientSuccess_ReturnsPublications()
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

    public class GetPublicationTests : ContentApiClientTests
    {

        [Fact]
        public async Task HttpClientBadRequest_ReturnsBadRequest()
        {
            var publicationId = Guid.NewGuid();

            _mockHttp.Expect(HttpMethod.Get, $"http://localhost/api/publications/{publicationId}")
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

            var response = await _contentApiClient.GetPublication(publicationId);

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            left.AssertValidationProblem(Errors.Error1);
        }

        [Fact]
        public async Task HttpClientNotFound_ReturnsNotFound()
        {
            var publicationId = Guid.NewGuid();

            _mockHttp.Expect(HttpMethod.Get, $"http://localhost/api/publications/{publicationId}")
                .Respond(
                HttpStatusCode.NotFound,
                JsonContent.Create(new ProblemDetails { Detail = "Not found details" }));

            var response = await _contentApiClient.GetPublication(publicationId);

            _mockHttp.VerifyNoOutstandingExpectation();

            var left = response.AssertLeft();
            left.AssertNotFoundObjectResult(new ProblemDetails { Detail = "Not found details" });
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
            var publicationId = Guid.NewGuid();

            _mockHttp.Expect(HttpMethod.Get, $"http://localhost/api/publications/{publicationId}")
                .Respond(responseStatusCode);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _contentApiClient.GetPublication(publicationId));

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task HttpClientSuccess_ReturnsPublication()
        {
            var result = new PublishedPublicationSummaryViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Test title",
                Slug = "test-slug",
                Summary = "Test summary",
                Published = DateTime.UtcNow,
            };

            _mockHttp.Expect(HttpMethod.Get, $"http://localhost/api/publications/{result.Id}")
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(result));

            var response = await _contentApiClient.GetPublication(result.Id);

            _mockHttp.VerifyNoOutstandingExpectation();

            var right = response.AssertRight();
            Assert.NotNull(right);
            Assert.Equal(result.Id, right.Id);
            Assert.Equal(result.Title, right.Title);
            Assert.Equal(result.Slug, right.Slug);
            Assert.Equal(result.Summary, right.Summary);
            Assert.Equal(result.Published, right.Published);
        }
    }

    private enum Errors
    {
        Error1,
    }
}
