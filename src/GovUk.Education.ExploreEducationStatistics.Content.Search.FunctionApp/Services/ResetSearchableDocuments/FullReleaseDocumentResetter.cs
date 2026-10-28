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

        // Delete all release documents
        await releaseDocumentRemover.RemoveAllReleaseDocuments(cancellationToken);

        // Get list of all live publications
        var allPublications = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Return list of slugs to send to the Release Document creator
        return new PerformResetResponse { AllPublications = allPublications };
    }
}
