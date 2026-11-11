namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

public record RefreshPublicationDataSetDocumentsRequest
{
    public required string PublicationSlug { get; init; }
}
