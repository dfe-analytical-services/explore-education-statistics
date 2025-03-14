using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureBlobStorage;

public class AzureBlobStorageIntegrationHelper
{
    public static async Task<Blob> DownloadAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
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

    public static async Task DeleteAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
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


