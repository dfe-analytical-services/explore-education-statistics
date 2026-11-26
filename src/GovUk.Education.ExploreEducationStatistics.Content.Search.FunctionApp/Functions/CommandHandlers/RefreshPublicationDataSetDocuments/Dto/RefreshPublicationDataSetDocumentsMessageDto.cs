namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;

public record RefreshPublicationDataSetDocumentsMessageDto
{
    public Guid? PublicationId { get; init; }
    public string? PublicationSlug { get; init; }
}
