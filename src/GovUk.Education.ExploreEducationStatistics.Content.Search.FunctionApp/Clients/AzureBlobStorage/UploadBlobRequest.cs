namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

/// <summary>
/// Request to upload a Blob to Azure storage account
/// </summary>
/// <param name="ContainerName">The name of the storage account container</param>
/// <param name="BlobName">The name (including any path) of the blob</param>
/// <param name="Blob">The contents and metadata to be uploaded</param>
public record UploadBlobRequest(string ContainerName, string BlobName, Blob Blob);