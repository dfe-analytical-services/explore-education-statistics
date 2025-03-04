using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public class AzureBlobStorageClient(BlobServiceClient blobServiceClient) : IAzureBlobStorageClient
{
    internal BlobServiceClient BlobServiceClient { get; } = blobServiceClient;

    public async Task UploadBlob(string containerName, string blobName, Blob blob, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        await using var stream = blob.Contents.ToStream();
        await blobClient.UploadAsync(
            stream,
            metadata: blob.Metadata,
            cancellationToken: cancellationToken);
    }
}
