using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public interface IFullSearchableDocumentReseter
{
    Task<RunResponse> Run(CancellationToken cancellationToken = default);
}

public class FullSearchableDocumentReseter(
    IContentApiClient contentApiClient,
    ISearchableDocumentRemover searchableDocumentRemover,
    ILogger<FullSearchableDocumentReseter> logger) : IFullSearchableDocumentReseter
{
    public async Task<RunResponse> Run(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Searchable Document Reset");
        
        // Delete all searchable documents
        await searchableDocumentRemover.RemoveAllSearchableDocuments(cancellationToken);
        
        // Get list of all live publications
        var allPublications = await contentApiClient.GetAllLivePublicationSlugs(cancellationToken);
        
        // Return list of slugs to send to the Searchable Document creator
        return new RunResponse { AllPublicationSlugs = allPublications };
    }
}

public class RunResponse
{
    public string[] AllPublicationSlugs { get; init; }
}
