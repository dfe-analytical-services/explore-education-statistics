namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

/// <summary>
/// Identifies a publication which has data set documents.
/// </summary>
public record DataSetDocumentPublication
{
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
}
