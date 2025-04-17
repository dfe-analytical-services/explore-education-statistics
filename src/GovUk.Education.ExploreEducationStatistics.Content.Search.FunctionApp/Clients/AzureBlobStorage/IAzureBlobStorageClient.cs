namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public interface IAzureBlobStorageClient
{
    /// <summary>
    /// Deletes a blob from the specified container if it exists.
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A a boolean indicating whether the blob was successfully deleted or did not exist.
    /// </returns>
    Task<bool> DeleteBlobIfExists(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a Blob to our configured Azure Blob storage account
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="blobName">The name (including any path) of the blob</param>
    /// <param name="blob">The contents and metadata to be uploaded</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadBlob(string containerName, string blobName, Blob blob, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check for the existence of a container in our configured Azure Blob Storage account
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ContainerExists(string containerName, CancellationToken cancellationToken = default);
}
