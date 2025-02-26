namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public interface IAzureBlobStorageClient
{
    Task UploadAsync(string containerName, string blobName, Blob blob, CancellationToken cancellationToken = default);
    Task<Blob> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
}
