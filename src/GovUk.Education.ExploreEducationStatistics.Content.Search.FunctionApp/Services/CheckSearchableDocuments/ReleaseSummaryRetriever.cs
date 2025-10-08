using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IReleaseSummaryRetriever
{
    Task<IList<ReleaseSummary>> GetAllPublishedReleaseSummaries(CancellationToken cancellationToken = default);
}

public class ReleaseSummaryRetriever(
    Func<IContentApiClient> contentApiClientFactory,
    ILogger<ReleaseSummaryRetriever> logger
) : IReleaseSummaryRetriever
{
    public async Task<IList<ReleaseSummary>> GetAllPublishedReleaseSummaries(
        CancellationToken cancellationToken = default
    )
    {
        // Get a list of all publication releases
        var contentApiClient = contentApiClientFactory();
        var allLivePublicationInfos = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Retrieve the release summary for each of the publications
        var releaseSummaries = await allLivePublicationInfos
            .ToAsyncEnumerable()
            .SelectAwait(async publicationInfo =>
            {
                try
                {
                    return await contentApiClient.GetReleaseSummary(
                        publicationInfo.PublicationSlug,
                        publicationInfo.LatestReleaseSlug,
                        cancellationToken
                    );
                }
                catch (UnableToGetReleaseSummaryForPublicationException ex)
                {
                    logger.LogError(
                        ex,
                        "Call to ContentApi GetReleaseSummary failed: {PublicationSlug} {LatestReleaseSlug}",
                        publicationInfo.PublicationSlug,
                        publicationInfo.LatestReleaseSlug
                    );
                    return null;
                }
            })
            .OfType<ReleaseSummary>()
            .ToArrayAsync(cancellationToken);

        return releaseSummaries;
    }
}
