using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IReleaseSummaryRetriever
{
    Task<IList<ReleaseSummary>> GetAllPublishedReleaseSummaries(
        CancellationToken cancellationToken = default);
}

public class ReleaseSummaryRetriever(Func<IContentApiClient> contentApiClientFactory) : IReleaseSummaryRetriever
{
    public async Task<IList<ReleaseSummary>> GetAllPublishedReleaseSummaries(
        CancellationToken cancellationToken = default)
    {
        // Get a list of all publication releases
        var contentApiClient = contentApiClientFactory();
        var allLivePublicationInfos = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Retrieve the release summary for each of the publications
        var releaseSummaries = 
            await allLivePublicationInfos
                .ToAsyncEnumerable()
                .SelectAwait<PublicationInfo, ReleaseSummary>(
                    async publicationInfo =>
                        await contentApiClient.GetReleaseSummary(
                            publicationInfo.PublicationSlug,
                            publicationInfo.LatestReleaseSlug,
                            cancellationToken))
                .ToArrayAsync(cancellationToken);
        
        return releaseSummaries;
    }
}
