namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public record CreatePublicationLatestReleaseSearchableDocumentResponse
{
    public required string PublicationSlug { get; init; }
    public required Guid ReleaseId { get; set; }
    public required string ReleaseSlug { get; set; }
    public required Guid ReleaseVersionId { get; init; }
    public required string BlobName { get; init; }
}
