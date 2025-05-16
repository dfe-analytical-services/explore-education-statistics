namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;

public record RefreshSearchableDocumentMessageDto
{
    public string? PublicationSlug { get; init; }
}
