using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public class FullSearchableDocumentResetter(
    IContentApiClient contentApiClient,
    ISearchableDocumentRemover searchableDocumentRemover,
    ILogger<FullSearchableDocumentResetter> logger
) : IFullSearchableDocumentResetter
{
    public async Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Searchable Document Reset");

        // Delete all searchable documents
        await searchableDocumentRemover.RemoveAllSearchableDocuments(cancellationToken);

        // Get list of all live publications
        var allPublications = await contentApiClient.GetAllLivePublicationInfos(cancellationToken);

        // Return list of slugs to send to the Searchable Document creator
        return new PerformResetResponse { AllPublications = allPublications };
    }
}
