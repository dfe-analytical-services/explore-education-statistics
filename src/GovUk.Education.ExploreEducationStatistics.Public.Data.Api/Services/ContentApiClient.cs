using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class ContentApiClient : IContentApiClient
{
    private readonly HttpClient httpClient;

    public ContentApiClient(HttpClient HttpClient)
    {
        httpClient=HttpClient;
    }

    public async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> ListPublications(int page, int pageSize, string? search = null, IEnumerable<Guid>? publicationIds = null)
    {
        var request = new PublicationsListPostRequest(
            ReleaseType: null,
            ThemeId: null,
            Search: search,
            Sort: null,
            Order: null,
            Page: page,
            PageSize: pageSize,
            PublicationIds: publicationIds);

        var response = await httpClient.PostAsJsonAsync("api/publications", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationSearchResultViewModel>>();
    }
}
