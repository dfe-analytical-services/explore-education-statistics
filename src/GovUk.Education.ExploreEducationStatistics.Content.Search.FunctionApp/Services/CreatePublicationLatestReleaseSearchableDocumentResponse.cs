namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public abstract record CreatePublicationLatestReleaseSearchableDocumentResponse
{
    public record Success(Guid ReleaseVersionId, string BlobName) : CreatePublicationLatestReleaseSearchableDocumentResponse();
    public record NotFound() : CreatePublicationLatestReleaseSearchableDocumentResponse();
    public record Error(string ErrorMessage) : CreatePublicationLatestReleaseSearchableDocumentResponse();
}
