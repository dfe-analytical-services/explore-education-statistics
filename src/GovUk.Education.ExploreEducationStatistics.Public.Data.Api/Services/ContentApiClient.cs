using System.Net;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class ContentApiClient(ILogger<ContentApiClient> logger, HttpClient httpClient) : IContentApiClient
{
    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<Guid>? publicationIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = new PublicationsListPostRequest(
            Search: search,
            Page: page,
            PageSize: pageSize,
            PublicationIds: publicationIds
        );

        var response = await httpClient.PostAsJsonAsync("api/publications", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content.ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken)
                    );

                default:
                    var message = await response.Content.ReadAsStringAsync(cancellationToken);

                    logger.LogError(
                        """
                        Failed to retrieve publications with status code: {statusCode}. 
                        Message: {message}",
                        """,
                        response.StatusCode,
                        message
                    );

                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        var publications = await response.Content.ReadFromJsonAsync<
            PaginatedListViewModel<PublicationSearchResultViewModel>
        >(cancellationToken);

        return publications ?? throw new NullReferenceException("Could not deserialize content API response.");
    }

    public async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync($"api/publications/{publicationId}/summary", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(
                        await response.Content.ReadFromJsonAsync<ValidationProblemViewModel>(cancellationToken)
                    );

                case HttpStatusCode.NotFound:
                    return new NotFoundResult();

                default:
                    var message = await response.Content.ReadAsStringAsync(cancellationToken);

                    logger.LogError(
                        """
                        Failed to retrieve publication '{publicationId}' with status code: {statusCode}. 
                        Message: {message}
                        """,
                        publicationId,
                        response.StatusCode,
                        message
                    );

                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        var publication = await response.Content.ReadFromJsonAsync<PublishedPublicationSummaryViewModel>(
            cancellationToken
        );

        return publication ?? throw new NullReferenceException("Could not deserialize from content API response.");
    }
}

// TODO To be removed by EES-6470 - Added in a temporary commit to avoid build errors
public record PublicationSearchResultViewModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public required string LatestReleaseSlug { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Theme { get; init; } = string.Empty;
    public DateTime Published { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public int Rank { get; set; }
}

// TODO To be removed by EES-6470 - Added in a temporary commit to avoid build errors
public record PublicationsListPostRequest(
    ReleaseType? ReleaseType = null,
    Guid? ThemeId = null,
    string? Search = null,
    PublicationsSortBy? Sort = null,
    SortDirection? SortDirection = null,
    int Page = 1,
    int PageSize = 10,
    IEnumerable<Guid>? PublicationIds = null
);

// TODO To be removed by EES-6470 - Added in a temporary commit to avoid build errors
public enum PublicationsSortBy
{
    Published,
    Relevance,
    Title,
}
