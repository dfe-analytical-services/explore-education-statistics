namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <summary>
/// The slugs identify the release in the Content API call when fetching its data set documents.
/// The ids are also needed, to check that the fetched data set documents all belong to the release.
/// </summary>
public record CreateReleaseDataSetDocumentsRequest
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string ReleaseSlug { get; init; }
}
