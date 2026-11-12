namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

public record RefreshPublicationDataSetDocumentsRequest
{
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
}
