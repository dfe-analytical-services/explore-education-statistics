namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <summary>
/// The slug identifies the publication in the Content API call when fetching its data set documents.
/// The id is also needed, to check that the fetched data set documents all belong to the publication.
/// </summary>
public record CreatePublicationDataSetDocumentsRequest
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }
}
