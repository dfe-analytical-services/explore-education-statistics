using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class ContentApiClient : IContentApiClient
{
    private readonly ILogger<ContentApiClient> _logger;
    private readonly HttpClient _httpClient;

    public ContentApiClient(ILogger<ContentApiClient> logger, HttpClient HttpClient)
    {
        _logger = logger;
        _httpClient = HttpClient;
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(int page, int pageSize, string? search = null, IEnumerable<Guid>? publicationIds = null)
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
            _logger.LogError($"Failed to retrieve publications.{Environment.NewLine}Status Code: {response.StatusCode}{Environment.NewLine}Message: {message}");

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(response.Content);
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        return new Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>(await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationSearchResultViewModel>>());
    }
}
