using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IReleaseSummaryRetriever
{
    Task<IList<ReleaseVersionSummary>> GetAllPublishedReleaseVersionSummaries(
        CancellationToken cancellationToken = default
    );
}

public class ReleaseSummaryRetriever(
    Func<IContentApiClient> contentApiClientFactory,
    ILogger<ReleaseSummaryRetriever> logger
) : IReleaseSummaryRetriever
{
    public async Task<IList<ReleaseVersionSummary>> GetAllPublishedReleaseVersionSummaries(
        CancellationToken cancellationToken = default
    )
    {
        // Retrieve all published publications, including details about the latest published releases for each
        var contentApiClient = contentApiClientFactory();
        var allLivePublicationInfos = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Get the latest published release version summary for each of the releases
        return await allLivePublicationInfos
            .ToAsyncEnumerable()
            .Select(
                async (publicationInfo, ct) =>
                {
                    try
                    {
                        return await contentApiClient.GetReleaseVersionSummary(
                            publicationInfo.PublicationSlug,
                            publicationInfo.LatestReleaseSlug,
                            cancellationToken: ct
                        );
                    }
                    catch (UnableToGetReleaseVersionSummaryException ex)
                    {
                        logger.LogError(
                            ex,
                            "Call to ContentApi GetReleaseVersionSummary failed: {PublicationSlug} {LatestReleaseSlug}",
                            publicationInfo.PublicationSlug,
                            publicationInfo.LatestReleaseSlug
                        );
                        return null;
                    }
                }
            )
            .OfType<ReleaseVersionSummary>()
            .ToArrayAsync(cancellationToken);
    }
}
