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
    public async Task<FullReleaseDocumentResetResponse> PerformReset(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Release Document Reset");

        // Get list of all live publications before deleting anything, so that a failure to get them does not leave an
        // empty container
        var publications = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Delete all release documents
        var removed = await releaseDocumentRemover.RemoveAllReleaseDocuments(cancellationToken);

        // Return the list of publications to send to the Release Document creator
        return new FullReleaseDocumentResetResponse
        {
            Publications = publications,
            RemovedBlobCount = removed.RemovedBlobNames.Count,
        };
    }
}
