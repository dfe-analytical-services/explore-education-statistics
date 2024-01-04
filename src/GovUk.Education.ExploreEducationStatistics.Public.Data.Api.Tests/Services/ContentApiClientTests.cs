using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public class ContentApiClientTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _client;
    private readonly Mock<ILogger<ContentApiClient>> _logger;
    private readonly ContentApiClient _contentApiClient;

    public ContentApiClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _client = _mockHttp.ToHttpClient();
        _client.BaseAddress = new Uri("http://localhost/");
        _logger = new Mock<ILogger<ContentApiClient>>();
        _contentApiClient = new ContentApiClient(_logger.Object, _client);
    }

    [Fact]
    public async Task ListPublications_HttpClientRespondsWithBadRequest_ReturnsBadRequestObjectResultAndLogsError()
    {
        _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
            .Respond(HttpStatusCode.BadRequest, new StringContent("test message"));

        var response = await _contentApiClient.ListPublications(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>());

        _mockHttp.VerifyNoOutstandingExpectation();

        _logger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"Failed to retrieve publications.{Environment.NewLine}Status Code: {HttpStatusCode.BadRequest}{Environment.NewLine}Message: test message"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var left = response.AssertLeft();
        Assert.IsType<BadRequestObjectResult>(left);
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task ListPublications_HttpClientRespondsWithUnsuccessfulStatusCode_ThrowsHttpRequestExceptionAndLogsError(HttpStatusCode responseStatusCode)
    {
        _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
            .Respond(responseStatusCode, new StringContent("test message"));

        var action = async () => await _contentApiClient.ListPublications(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>());

        await Assert.ThrowsAsync<HttpRequestException>(action);

        _mockHttp.VerifyNoOutstandingExpectation();

        _logger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"Failed to retrieve publications.{Environment.NewLine}Status Code: {responseStatusCode}{Environment.NewLine}Message: test message"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ListPublications_HttpClientRespondsWithSuccess_ReturnsPublications()
    {
        var results = new List<PublicationSearchResultViewModel>()
        {
            new() {
                Id = Guid.NewGuid(),
            }
        };
        var responseBody = new PaginatedListViewModel<PublicationSearchResultViewModel>(results, results.Count, 1, results.Count);

        _mockHttp.Expect(HttpMethod.Post, "http://localhost/api/publications")
            .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(responseBody));

        var response = await _contentApiClient.ListPublications(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>());

        _mockHttp.VerifyNoOutstandingExpectation();

        _logger.Verify(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        var right = response.AssertRight();
        var publication = Assert.Single(right.Results);
        Assert.Equal(results.Single().Id, publication.Id);
    }
}
