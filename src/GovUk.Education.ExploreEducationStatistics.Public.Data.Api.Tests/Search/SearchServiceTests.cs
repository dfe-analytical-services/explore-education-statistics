using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Search;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Search;

public abstract class SearchServiceTests
{
    public class SearchPublicationsTests : SearchServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("pupil absence")]
        public async Task WhenClientRequestIsSuccessful_ReturnsPaginatedResults(string? searchText)
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            Guid[] publicationIds = [Guid.NewGuid(), Guid.NewGuid()];
            PublicationSearchResult[] documents =
            [
                new()
                {
                    PublicationId = publicationIds[0],
                    PublicationSlug = "publication-1",
                    Published = DateTimeOffset.UtcNow.AddDays(-1),
                    Summary = "Publication 1 Summary",
                    Title = "Publication 1 Title",
                },
                new()
                {
                    PublicationId = publicationIds[1],
                    PublicationSlug = "publication-2",
                    Published = DateTimeOffset.UtcNow.AddDays(-2),
                    Summary = "Publication 2 Summary",
                    Title = "Publication 2 Title",
                },
            ];

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [
                    SearchModelFactory.SearchResult(document: documents[0], score: 1.0, highlights: null),
                    SearchModelFactory.SearchResult(document: documents[1], score: 0.9, highlights: null),
                ],
                totalCount: 2,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        searchText,
                        It.IsAny<SearchOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds,
                searchText: searchText
            );

            // Assert
            var pagedResult = outcome.AssertRight();
            pagedResult.AssertHasExpectedPagingAndResultCount(
                expectedTotalResults: 2,
                expectedPage: page,
                expectedPageSize: pageSize
            );
            Assert.Equal(documents, pagedResult.Results);
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenClientRequestIsCalled_UsesCorrectSearchOptions()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            Guid[] publicationIds = [Guid.NewGuid(), Guid.NewGuid()];
            PublicationSearchResult[] documents =
            [
                new()
                {
                    PublicationId = publicationIds[0],
                    PublicationSlug = "publication-1",
                    Published = DateTimeOffset.UtcNow.AddDays(-1),
                    Summary = "Publication 1 Summary",
                    Title = "Publication 1 Title",
                },
                new()
                {
                    PublicationId = publicationIds[1],
                    PublicationSlug = "publication-2",
                    Published = DateTimeOffset.UtcNow.AddDays(-2),
                    Summary = "Publication 2 Summary",
                    Title = "Publication 2 Title",
                },
            ];

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [
                    SearchModelFactory.SearchResult(document: documents[0], score: 1.0, highlights: null),
                    SearchModelFactory.SearchResult(document: documents[1], score: 0.9, highlights: null),
                ],
                totalCount: 2,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var expectedSearchOptions = new SearchOptions
            {
                Filter = $"search.in(publicationId, '{publicationIds[0]},{publicationIds[1]}', ',')",
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Semantic,
                ScoringProfile = "scoring-profile-1",
                SemanticSearch = new SemanticSearchOptions { SemanticConfigurationName = "semantic-configuration-1" },
                Size = pageSize,
                Skip = 0,
            };

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            // Verify that the method is called with the expected options
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.Is<SearchOptions>(options =>
                            options.Filter == expectedSearchOptions.Filter
                            && options.IncludeTotalCount == expectedSearchOptions.IncludeTotalCount
                            && options.QueryType == expectedSearchOptions.QueryType
                            && options.ScoringProfile == expectedSearchOptions.ScoringProfile
                            && options.SemanticSearch.SemanticConfigurationName
                                == expectedSearchOptions.SemanticSearch.SemanticConfigurationName
                            && options.Size == expectedSearchOptions.Size
                            && options.Skip == expectedSearchOptions.Skip
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds
            );

            // Assert
            outcome.AssertRight();
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenPageIsNotFirst_UsesCorrectSkipAndSizeOptions()
        {
            // Arrange
            const int page = 3;
            const int pageSize = 100;
            Guid[] publicationIds = [Guid.NewGuid()];
            var document = new PublicationSearchResult
            {
                PublicationId = publicationIds[0],
                PublicationSlug = "publication-1",
                Published = DateTimeOffset.UtcNow.AddDays(-1),
                Summary = "Publication 1 Summary",
                Title = "Publication 1 Title",
            };

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [SearchModelFactory.SearchResult(document: document, score: 1.0, highlights: null)],
                totalCount: 1,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            // Verify that the method is called with the expected size and skip options
            // Expecting size of 100, and skip of 200 (page 3, size 100 should mean skipping the first 200 results)
            const int expectedSkipOption = 200;
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.Is<SearchOptions>(options => options.Size == pageSize && options.Skip == expectedSkipOption),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds
            );

            // Assert
            var pagedResult = outcome.AssertRight();
            pagedResult.AssertHasExpectedPagingAndResultCount(
                expectedTotalResults: 1,
                expectedPage: page,
                expectedPageSize: pageSize
            );
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenPublicationIdsIsEmpty_UsesCorrectFilterOption()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            Guid[] publicationIds = [];

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [],
                totalCount: 0,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            // Verify that the method is called with the expected filter option
            // Expect the filter to be present, and correctly formed for an empty list of publication IDs.
            // Omitting the filter would return all publications, including those unrelated to Public API data sets.
            const string expectedFilterOption = "search.in(publicationId, '', ',')";
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.Is<SearchOptions>(options => options.Filter == expectedFilterOption),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds
            );

            // Assert
            outcome.AssertRight();
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenClientRequestReturnsNoResults_ReturnsEmptyPage()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            Guid[] publicationIds = [Guid.NewGuid(), Guid.NewGuid()];

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [],
                totalCount: 0,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.IsAny<SearchOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds
            );

            // Assert
            var pagedResult = outcome.AssertRight();
            pagedResult.AssertHasExpectedPagingAndResultCount(
                expectedTotalResults: 0,
                expectedPage: page,
                expectedPageSize: pageSize
            );
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenClientRequestThrowsException_ReturnsServerError()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 1;
            Guid[] publicationIds = [Guid.NewGuid()];
            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.IsAny<SearchOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new RequestFailedException("Search request failed"));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act
            var outcome = await service.SearchPublications(
                page: page,
                pageSize: pageSize,
                publicationIds: publicationIds
            );

            // Assert
            outcome.AssertInternalServerError();
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }

        [Fact]
        public async Task WhenSearchResultTotalCountIsNull_ThrowsException()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 1;
            Guid[] publicationIds = [Guid.NewGuid()];
            var document = new PublicationSearchResult
            {
                PublicationId = publicationIds[0],
                PublicationSlug = "publication-1",
                Published = DateTimeOffset.UtcNow.AddDays(-1),
                Summary = "Publication 1 Summary",
                Title = "Publication 1 Title",
            };

            var response = new Mock<Response>();
            var searchResults = SearchModelFactory.SearchResults<PublicationSearchResult>(
                [SearchModelFactory.SearchResult(document: document, score: 1.0, highlights: null)],
                totalCount: null,
                facets: null,
                coverage: null,
                rawResponse: response.Object
            );

            var searchClient = new Mock<SearchClient>(MockBehavior.Strict);
            searchClient
                .Setup(client =>
                    client.SearchAsync<PublicationSearchResult>(
                        It.IsAny<string>(),
                        It.IsAny<SearchOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Response.FromValue(searchResults, response.Object));

            var searchClientFactory = new Mock<IAzureClientFactory<SearchClient>>(MockBehavior.Strict);
            searchClientFactory
                .Setup(factory => factory.CreateClient("PublicationsSearchClient"))
                .Returns(searchClient.Object);

            var service = BuildService(searchClientFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SearchPublications(page: page, pageSize: pageSize, publicationIds: publicationIds)
            );
            Assert.Equal(
                "Search response did not contain TotalCount even though IncludeTotalCount was requested.",
                exception.Message
            );
            MockUtils.VerifyAllMocks(searchClientFactory, searchClient);
        }
    }

    private static SearchService BuildService(IAzureClientFactory<SearchClient>? searchClientFactory = null) =>
        new(
            searchClientFactory ?? Mock.Of<IAzureClientFactory<SearchClient>>(MockBehavior.Strict),
            Mock.Of<ILogger<SearchService>>()
        );
}
