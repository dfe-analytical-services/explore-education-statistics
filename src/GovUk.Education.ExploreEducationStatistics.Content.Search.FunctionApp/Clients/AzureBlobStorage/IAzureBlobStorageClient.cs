namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public interface IAzureBlobStorageClient
{
    /// <summary>
    /// Upload a Blob to Azure storage account
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<UploadBlobResponse> UploadBlob(UploadBlobRequest request, CancellationToken cancellationToken = default);
}
