namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;

public record SearchableDocumentCreatedMessageDto
{
    public string? PublicationSlug { get; init; }
    public Guid? ReleaseId { get; init; }
    public string? ReleaseSlug { get; init; }
    public Guid? ReleaseVersionId { get; init; }
    public string? BlobName { get; init; }
}
