using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

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
            var blobName = releaseInfo.ReleaseId.ToString();
            results[releaseInfo.ReleaseId] =
                await azureBlobStorageClient.DeleteBlobIfExists(
                    appOptions.Value.SearchableDocumentsContainerName,
                    blobName,
                    cancellationToken);
        }

        return new RemovePublicationSearchableDocumentsResponse(results);
    }
}
