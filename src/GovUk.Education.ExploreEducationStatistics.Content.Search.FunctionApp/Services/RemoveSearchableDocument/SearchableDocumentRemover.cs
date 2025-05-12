using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;

/// <summary>
/// This service provides a way of deleting all searchable documents that may exist for a publication.
/// </summary>
internal class SearchableDocumentRemover(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions) : ISearchableDocumentRemover
{
    public async Task<RemovePublicationSearchableDocumentsResponse> RemovePublicationSearchableDocuments(
        RemovePublicationSearchableDocumentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var releaseInfos = await contentApiClient.GetReleasesForPublication(
            request.PublicationSlug,
            cancellationToken);

        var results = new Dictionary<Guid, bool>();

        foreach (var releaseInfo in releaseInfos)
        {
            var result = await RemoveSearchableDocument(new RemoveSearchableDocumentRequest{ ReleaseId = releaseInfo.ReleaseId }, cancellationToken);
            results[releaseInfo.ReleaseId] = result.Success;
        }

        return new RemovePublicationSearchableDocumentsResponse(results);
    }

    public async Task<RemoveSearchableDocumentResponse> RemoveSearchableDocument(
        RemoveSearchableDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var blobName = request.ReleaseId.ToString();

        var success = await azureBlobStorageClient.DeleteBlobIfExists(
            appOptions.Value.SearchableDocumentsContainerName,
            blobName,
            cancellationToken);

        return new RemoveSearchableDocumentResponse(success);
    }

    public async Task RemoveAllSearchableDocuments(CancellationToken cancellationToken = default)
    {
        await azureBlobStorageClient.DeleteAllBlobsFromContainer(
            appOptions.Value.SearchableDocumentsContainerName,
            cancellationToken);
    }
}
