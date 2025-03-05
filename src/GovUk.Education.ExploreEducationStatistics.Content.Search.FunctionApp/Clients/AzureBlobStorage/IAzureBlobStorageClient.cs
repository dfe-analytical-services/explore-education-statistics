namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public interface IAzureBlobStorageClient
{
    /// <summary>
    /// Upload a Blob to Azure storage account
    /// </summary>
    /// <param name="containerName">The name of the storage account container</param>
    /// <param name="blobName">The name (including any path) of the blob</param>
    /// <param name="blob">The contents and metadata to be uploaded</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadBlob(string containerName, string blobName, Blob blob, CancellationToken cancellationToken = default);
}
