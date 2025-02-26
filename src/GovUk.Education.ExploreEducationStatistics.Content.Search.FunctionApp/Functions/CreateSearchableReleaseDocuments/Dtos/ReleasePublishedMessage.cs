namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;

public record ReleasePublishedMessage(string ReleaseSlug);

public record SearchDocumentCreatedMessage
{
    public required string PublicationSlug { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required string BlobName { get; init; }
}
