using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public class FullReleaseDocumentResetter(
    IContentApiClient contentApiClient,
    IReleaseDocumentRemover releaseDocumentRemover,
    ILogger<FullReleaseDocumentResetter> logger
) : IFullReleaseDocumentResetter
{
    public async Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Release Document Reset");

        // Get list of all live publications before deleting anything, so that a failure to get them does not leave an
        // empty container with nothing queued to fill it again
        var allPublications = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Delete all release documents
        await releaseDocumentRemover.RemoveAllReleaseDocuments(cancellationToken);

        // Return list of slugs to send to the Release Document creator
        return new PerformResetResponse { AllPublications = allPublications };
    }
}
