namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;

public record RemoveReleaseDocumentMessageDto
{
    public Guid? ReleaseId { get; init; }
}
