using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;

/// <summary>
/// This service provides a way of deleting all release documents that may exist for a publication.
/// </summary>
internal class ReleaseDocumentRemover(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions
) : IReleaseDocumentRemover
{
    public async Task<RemovePublicationReleaseDocumentsResponse> RemovePublicationReleaseDocuments(
        RemovePublicationReleaseDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var releaseIds = await contentApiClient.GetPublicationReleaseIds(request.PublicationSlug, cancellationToken);

        var results = new Dictionary<Guid, bool>();

        foreach (var releaseId in releaseIds)
        {
            var result = await RemoveReleaseDocument(
                new RemoveReleaseDocumentRequest { ReleaseId = releaseId },
                cancellationToken
            );
            results[releaseId] = result.Success;
        }

        return new RemovePublicationReleaseDocumentsResponse(results);
    }

    public async Task<RemoveReleaseDocumentResponse> RemoveReleaseDocument(
        RemoveReleaseDocumentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var blobName = request.ReleaseId.ToString();

        var success = await azureBlobStorageClient.DeleteBlobIfExists(
            appOptions.Value.ReleaseDocumentsContainerName,
            blobName,
            cancellationToken
        );

        return new RemoveReleaseDocumentResponse(success);
    }

    public async Task RemoveAllReleaseDocuments(CancellationToken cancellationToken = default)
    {
        await azureBlobStorageClient.DeleteAllBlobsFromContainer(
            appOptions.Value.ReleaseDocumentsContainerName,
            cancellationToken
        );
    }
}
