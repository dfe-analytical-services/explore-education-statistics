using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Search;

public class SearchService(IAzureClientFactory<SearchClient> searchClientFactory, ILogger<SearchService> logger)
    : ISearchService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResult>>> SearchPublications(
        int page,
        int pageSize,
        IEnumerable<Guid> publicationIds,
        string? searchText = null,
        CancellationToken cancellationToken = default
    )
    {
        var searchClient = searchClientFactory.CreateClient("PublicationsSearchClient");

        var options = new SearchOptions
        {
            Filter = CreatePublicationIdsFilter(publicationIds),
            IncludeTotalCount = true,
            QueryType = SearchQueryType.Semantic,
            ScoringProfile = "scoring-profile-1",
            SemanticSearch = new SemanticSearchOptions { SemanticConfigurationName = "semantic-configuration-1" },
            Size = pageSize,
            Skip = (page - 1) * pageSize,
        };

        try
        {
            SearchResults<PublicationSearchResult> response = await searchClient.SearchAsync<PublicationSearchResult>(
                searchText,
                options,
                cancellationToken
            );

            var totalCount = Convert.ToInt32(
                response.TotalCount
                    ?? throw new InvalidOperationException(
                        "Search response did not contain TotalCount even though IncludeTotalCount was requested."
                    )
            );

            var allDocuments = await response
                .GetResultsAsync()
                .Select(result => result.Document)
                .ToListAsync(cancellationToken: cancellationToken);

            return new PaginatedListViewModel<PublicationSearchResult>(allDocuments, totalCount, page, pageSize);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(
                """
                Failed to search publications.
                Message: {message}",
                """,
                ex.Message
            );
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private static string CreatePublicationIdsFilter(IEnumerable<Guid> publicationIds)
    {
        var valueList = string.Join(',', publicationIds);
        return SearchFilter.Create($"search.in(publicationId, {valueList}, ',')");
    }
}
