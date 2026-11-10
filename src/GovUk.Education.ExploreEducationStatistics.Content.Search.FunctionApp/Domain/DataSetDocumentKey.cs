namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

/// <summary>
/// Identifies a data set document. There is one per data file in the latest published version of each release.
/// </summary>
public record DataSetDocumentKey
{
    public required Guid PublicationId { get; init; }
    public required Guid ReleaseId { get; init; }
    public required Guid FileId { get; init; }

    /// <summary>
    /// The name of the blob holding the data set document. The publication and release prefixes allow all the
    /// documents of a publication or release to be listed or removed by prefix.
    /// </summary>
    public string BlobName => $"{PublicationId}/{ReleaseId}/{FileId}.json";
}
