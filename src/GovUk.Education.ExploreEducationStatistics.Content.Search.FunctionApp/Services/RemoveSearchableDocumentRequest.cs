namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public record RemoveSearchableDocumentRequest
{
    public required Guid ReleaseId { get; init; }
}
