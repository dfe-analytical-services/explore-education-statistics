namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;

public record RefreshPublicationDataSetDocumentsMessageDto
{
    public string? PublicationSlug { get; init; }
}
