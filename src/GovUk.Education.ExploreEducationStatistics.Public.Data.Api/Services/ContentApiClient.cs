using StringExtensions = GovUk.Education.ExploreEducationStatistics.Common.Extensions.StringExtensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class ContentApiClient : IContentApiClient
{
    private readonly ILogger<ContentApiClient> _logger;
    private readonly HttpClient _httpClient;

    public ContentApiClient(ILogger<ContentApiClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page, 
        int pageSize, 
        string? search = null, 
        IEnumerable<Guid>? publicationIds = null)
    {
        var request = new PublicationsListPostRequest(
            Search: search,
            Page: page,
            PageSize: pageSize,
            PublicationIds: publicationIds);

        var response = await _httpClient.PostAsJsonAsync("api/publications", request);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            _logger.LogError(StringExtensions.TrimIndent(
                $"""
                 Failed to retrieve publications with status code: {response.StatusCode}. Message:
                 {message}
                 """));

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(await response.Content.ReadFromJsonAsync<ValidationProblemDetails>());
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        var publications =
            await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationSearchResultViewModel>>();

        return publications
            ?? throw new Exception("Could not deserialize publications from content API.");
    }

    public async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetPublication(Guid publicationId)
    {
        var response = await _httpClient.GetAsync($"api/publications/{publicationId}");

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            _logger.LogError(StringExtensions.TrimIndent(
                $"""
                 Failed to retrieve publication '{publicationId}' with status code: {response.StatusCode}. Message:
                 {message}
                 """));

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(await response.Content.ReadFromJsonAsync<ValidationProblemDetails>());
                case HttpStatusCode.NotFound:
                    return new NotFoundObjectResult(await response.Content.ReadFromJsonAsync<ProblemDetails>());
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        var publications = await response.Content.ReadFromJsonAsync<PublishedPublicationSummaryViewModel>();

        return publications
            ?? throw new Exception("Could not deserialize publication from content API.");
    }
}
