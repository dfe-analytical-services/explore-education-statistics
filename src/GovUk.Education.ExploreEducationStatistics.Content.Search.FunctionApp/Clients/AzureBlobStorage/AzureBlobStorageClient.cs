using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public class AzureBlobStorageClient(BlobServiceClient blobServiceClient) : IAzureBlobStorageClient
{
    internal BlobServiceClient BlobServiceClient { get; } = blobServiceClient;

    public async Task UploadAsync(string containerName, string blobName, Blob blob, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        await using var stream = GenerateStreamFromString(blob.Contents);
        await blobClient.UploadAsync(
            stream,
            metadata: blob.Metadata,
            cancellationToken: cancellationToken);
    }

    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public async Task<Blob> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        
        var existsResponse = await blobClient.ExistsAsync(cancellationToken);
        if (existsResponse.Value == false)
        {
            throw new AzureBlobStorageNotFoundException(containerName, blobName);
        }

        Response<BlobDownloadInfo>? response;
        try
        {
            response = await blobClient.DownloadAsync(cancellationToken);
            if (!response.HasValue)
            {
                throw new AzureBlobStorageException(containerName, blobName, $"Response was empty.");
            }
        }
        catch (Exception e)
        {
            throw new AzureBlobStorageException(containerName, blobName, e.Message);
        }

        using var streamReader = new StreamReader(response.Value.Content);
        var content = await streamReader.ReadToEndAsync(cancellationToken);
        var metadata = response.Value.Details.Metadata;
        return new Blob(content, metadata);
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        var existsResponse = await blobClient.ExistsAsync(cancellationToken);
        if (existsResponse.Value == false)
        {
            // If the blob is not found, treat that as success
            return;
        }

        try
        {
            var response = await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            if (response.IsError)
            {
                throw new Exception($"Blob \"{blobName}\" could not be deleted from container \"{containerName}\". {response.ReasonPhrase}({response.Status})");
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Blob \"{blobName}\" could not be deleted from container \"{containerName}\". {e.Message}");
        }
    }
}
