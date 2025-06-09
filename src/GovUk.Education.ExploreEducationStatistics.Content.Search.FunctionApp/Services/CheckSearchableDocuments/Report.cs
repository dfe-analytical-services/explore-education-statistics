namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record Report
{
    public required int OkBlobCount { get; init; }
    public required int TotalBlobCount { get; set; }
    public required int TotalPublicationCount { get; set; }
    public required string[] MissingBlobs { get; set; }
    public required string[] ExtraneousBlobs { get; set; }
    public required ReleaseSummaryViewModel[] MissingBlobSummaries { get; set; }
}