namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <summary>
/// The slugs identify the release to the Content API. The ids are also needed, so that the release's existing data
/// set documents can be found even when it no longer has any data files.
/// </summary>
public record RefreshReleaseDataSetDocumentsRequest
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string ReleaseSlug { get; init; }
}
