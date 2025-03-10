using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public class AzureBlobStorageClient(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobStorageClient> logger) : IAzureBlobStorageClient
{
    internal BlobServiceClient BlobServiceClient { get; } = blobServiceClient;

    public async Task UploadBlob(
        string containerName,
        string blobName,
        Blob blob,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        await using var stream = blob.Contents.ToStream();
        try
        {
            await blobClient.UploadAsync(
                stream,
                metadata: blob.Metadata,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading {ContainerName}/{BlobName}. Blob: {@Blob}", containerName, blobName, blob);
            throw;
        }
    }

    public async Task<bool> ContainerExists(string containerName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        return await blobContainerClient.ExistsAsync(cancellationToken);
    }
}
