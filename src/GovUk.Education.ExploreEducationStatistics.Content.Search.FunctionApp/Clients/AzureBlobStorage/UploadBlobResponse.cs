namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

public abstract record UploadBlobResponse
{
    public record Success() : UploadBlobResponse;

    public record Error(string ErrorMessage) : UploadBlobResponse;
}
