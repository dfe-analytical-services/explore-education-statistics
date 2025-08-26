using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class ContentApiClient(ILogger<ContentApiClient> logger, HttpClient httpClient) : IContentApiClient
{
    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<Guid>? publicationIds = null,
        CancellationToken cancellationToken = default)
    {
        var request = new PublicationsListPostRequest(
            Search: search,
            Page: page,
            PageSize: pageSize,
            PublicationIds: publicationIds);

        var response = await httpClient
            .PostAsJsonAsync("api/publications", request, cancellationToken);

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

        var publications = await response.Content
            .ReadFromJsonAsync<PaginatedListViewModel<PublicationSearchResultViewModel>>(cancellationToken);

        return publications
               ?? throw new NullReferenceException("Could not deserialize content API response.");
    }

    public async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .GetAsync($"api/publications/{publicationId}/summary", cancellationToken);

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

        var publication = await response.Content
            .ReadFromJsonAsync<PublishedPublicationSummaryViewModel>(cancellationToken);

        return publication
               ?? throw new NullReferenceException("Could not deserialize from content API response.");
    }
}
