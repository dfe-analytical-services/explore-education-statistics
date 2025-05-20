namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;

public record RemoveSearchableDocumentRequest
{
    public required Guid ReleaseId { get; init; }
}
