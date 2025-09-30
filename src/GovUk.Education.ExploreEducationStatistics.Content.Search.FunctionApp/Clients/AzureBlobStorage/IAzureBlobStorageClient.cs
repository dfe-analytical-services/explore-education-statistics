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
    Task<bool> DeleteBlobIfExists(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Upload a Blob to our configured Azure Blob storage account
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="blobName">The name (including any path) of the blob</param>
    /// <param name="blob">The contents and metadata to be uploaded</param>
    /// <param name="contentType">The media type of the blob before any content encoding is applied</param>
    /// <param name="contentEncoding">The content encodings which have been applied to the blob, if any</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadBlob(
        string containerName,
        string blobName,
        Blob blob,
        string contentType,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Check for the existence of a container in our configured Azure Blob Storage account
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<bool> ContainerExists(string containerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all blobs from the specified container
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAllBlobsFromContainer(
        string containerName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get a list of all blobs in a container
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An array of blob names</returns>
    Task<IList<string>> ListBlobsInContainer(
        string containerName,
        CancellationToken cancellationToken = default
    );
}
