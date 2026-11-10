namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record DataSetDocumentsCheckReport
{
    public required int OkBlobCount { get; init; }
    public required int TotalBlobCount { get; init; }
    public required int TotalExpectedCount { get; init; }

    /// <summary>
    /// The names of the blobs that are expected but don't exist, in the form {publicationId}/{releaseId}/{fileId}.json
    /// </summary>
    public required string[] MissingBlobs { get; init; }

    /// <summary>
    /// The names of the blobs that exist but aren't expected.
    /// </summary>
    public required string[] ExtraneousBlobs { get; init; }
}
