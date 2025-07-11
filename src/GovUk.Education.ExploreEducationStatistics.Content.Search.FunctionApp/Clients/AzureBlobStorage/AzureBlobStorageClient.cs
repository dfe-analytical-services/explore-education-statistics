using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

[ExcludeFromCodeCoverage]
public class AzureBlobStorageClient(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobStorageClient> logger) : IAzureBlobStorageClient
{
    internal BlobServiceClient BlobServiceClient { get; } = blobServiceClient;

    public async Task<bool> DeleteBlobIfExists(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            return await blobContainerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete blob {ContainerName}/{BlobName}", containerName, blobName);
            throw;
        }
    }

    public async Task UploadBlob(
        string containerName,
        string blobName,
        Blob blob,
        string contentType,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        var httpHeaders = new BlobHttpHeaders
        {
            ContentEncoding = contentEncoding,
            ContentType = contentType
        };

        await using var stream = blob.Contents.ToStream();
        try
        {
            await blobClient.UploadAsync(
                stream,
                metadata: blob.Metadata,
                httpHeaders: httpHeaders,
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

    public async Task DeleteAllBlobsFromContainer(string containerName, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var blobNames = await GetAllBlobNames(blobContainerClient, cancellationToken);

        await Task.WhenAll(
            blobNames
                .Select(blobName => 
                    blobContainerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken)));
    }

    public async Task<IList<string>> ListBlobsInContainer(
        string containerName,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        return await GetAllBlobNames(blobContainerClient, cancellationToken);
    }
    
    private static async Task<IList<string>> GetAllBlobNames(
        BlobContainerClient blobContainerClient,
        CancellationToken cancellationToken)
    {
        var asyncPageable = blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken);
        var blobNames = new List<string>();
        await foreach (var blobItem in asyncPageable)
        {
            blobNames.Add(blobItem.Name);
        }
        return blobNames;
    }
}
