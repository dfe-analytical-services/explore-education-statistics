namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments.Dto;

public record RefreshReleaseDataSetDocumentsMessageDto
{
    public Guid? PublicationId { get; init; }
    public string? PublicationSlug { get; init; }
    public Guid? ReleaseId { get; init; }
    public string? ReleaseSlug { get; init; }
}
