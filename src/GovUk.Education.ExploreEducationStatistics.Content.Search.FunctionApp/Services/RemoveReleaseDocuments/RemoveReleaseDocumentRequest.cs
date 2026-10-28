namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;

public record RemoveReleaseDocumentRequest
{
    public required Guid ReleaseId { get; init; }
}
