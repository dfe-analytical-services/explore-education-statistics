using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;

/// <summary>
/// This service removes data set documents. It finds them by blob name alone, so that the documents of a publication
/// or release can still be removed after it has been deleted.
/// </summary>
internal class DataSetDocumentRemover(IAzureBlobStorageClient azureBlobStorageClient, IOptions<AppOptions> appOptions)
    : IDataSetDocumentRemover
{
    public async Task<RemoveDataSetDocumentsResponse> RemoveDataSetDocuments(
        RemoveDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var blobNames = await GetBlobNames(request, cancellationToken);
        var blobNamesToKeep = request.Except.Select(key => key.BlobName).ToHashSet();

        var deletions = await Task.WhenAll(
            blobNames
                .Where(blobName => !blobNamesToKeep.Contains(blobName))
                .Select(async blobName =>
                    (
                        BlobName: blobName,
                        Deleted: await azureBlobStorageClient.DeleteBlobIfExists(
                            appOptions.Value.DataSetDocumentsContainerName,
                            blobName,
                            cancellationToken
                        )
                    )
                )
        );

        return new RemoveDataSetDocumentsResponse([
            .. deletions.Where(deletion => deletion.Deleted).Select(deletion => deletion.BlobName),
        ]);
    }

    private async Task<IList<string>> GetBlobNames(
        RemoveDataSetDocumentsRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.FileId is { } fileId)
        {
            var releaseId =
                request.ReleaseId
                ?? throw new ArgumentException(
                    "A release id is required to remove the data set document of a data file",
                    nameof(request)
                );

            var key = new DataSetDocumentKey
            {
                PublicationId = request.PublicationId,
                ReleaseId = releaseId,
                FileId = fileId,
            };
            return [key.BlobName];
        }

        return await azureBlobStorageClient.ListBlobsInContainer(
            appOptions.Value.DataSetDocumentsContainerName,
            DataSetDocumentKey.BlobNamePrefix(request.PublicationId, request.ReleaseId),
            cancellationToken
        );
    }
}
