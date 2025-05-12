using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public interface IFullSearchableDocumentResetter
{
    /// <summary>
    /// Delete all Searchable Documents from Azure storage, and return a list of all live publications.
    /// </summary>
    Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default);
}

public class FullSearchableDocumentResetter(
    IContentApiClient contentApiClient,
    ISearchableDocumentRemover searchableDocumentRemover,
    ILogger<FullSearchableDocumentResetter> logger) : IFullSearchableDocumentResetter
{
    public async Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Searchable Document Reset");
        
        // Delete all searchable documents
        await searchableDocumentRemover.RemoveAllSearchableDocuments(cancellationToken);
        
        // Get list of all live publications
        var allPublications = await contentApiClient.GetAllLivePublicationSlugs(cancellationToken);
        
        // Return list of slugs to send to the Searchable Document creator
        return new PerformResetResponse { AllPublications = allPublications };
    }
}

public class PerformResetResponse
{
    public PublicationInfo[] AllPublications { get; init; }
}
