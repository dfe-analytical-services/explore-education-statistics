namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal interface IContentApiClient
{
    Task<GetResponse> GetPublicationLatestReleaseSearchableDocumentAsync(GetRequest request, CancellationToken cancellationToken = default);
}
