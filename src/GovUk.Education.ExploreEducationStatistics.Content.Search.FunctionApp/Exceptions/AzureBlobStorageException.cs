namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

public class AzureBlobStorageException : Exception
{
    public string ContainerName { get; }
    public string BlobName { get; }

    public AzureBlobStorageException(string containerName, string blobName, string errorReason) : base(
        $"Error with Blob \"{blobName}\" in container \"{containerName}\": ${errorReason}")
    {
        ContainerName = containerName;
        BlobName = blobName;
    }
}
