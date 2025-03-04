using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public class AzureBlobStorageClient(BlobServiceClient blobServiceClient) : IAzureBlobStorageClient
{
    internal BlobServiceClient BlobServiceClient { get; } = blobServiceClient;

    public async Task<UploadBlobResponse> UploadBlob(UploadBlobRequest request, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(request.ContainerName);
        var blobClient = blobContainerClient.GetBlobClient(request.BlobName);

        await using var stream = request.Blob.Contents.ToStream();
        try
        {
            await blobClient.UploadAsync(
                stream,
                metadata: request.Blob.Metadata,
                cancellationToken: cancellationToken);
            
            return new UploadBlobResponse.Success();
        }
        catch (Exception e)
        {
            return new UploadBlobResponse.Error(e.Message);
        }
    }
}
