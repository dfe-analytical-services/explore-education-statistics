namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

public interface IContentApiClient
{
    Task<ReleaseSearchViewModelDto> GetPublicationLatestReleaseSearchViewModelAsync(string publicationSlug, CancellationToken cancellationToken = default);
}
