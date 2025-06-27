namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record CheckSearchableDocumentsReport
{
    public required int OkBlobCount { get; init; }
    public required int TotalBlobCount { get; init; }
    public required int TotalPublicationCount { get; init; }
    public required string[] MissingBlobs { get; init; }
    public required string[] ExtraneousBlobs { get; init; }
    public required ReleaseSummaryViewModel[] MissingBlobSummaries { get; init; }
}
