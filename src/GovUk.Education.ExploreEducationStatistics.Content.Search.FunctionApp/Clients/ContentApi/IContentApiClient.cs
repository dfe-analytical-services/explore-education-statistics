namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal interface IContentApiClient
{
    Task<GetResponse> GetPublicationLatestReleaseSearchViewModelAsync(GetRequest request, CancellationToken cancellationToken = default);
}
