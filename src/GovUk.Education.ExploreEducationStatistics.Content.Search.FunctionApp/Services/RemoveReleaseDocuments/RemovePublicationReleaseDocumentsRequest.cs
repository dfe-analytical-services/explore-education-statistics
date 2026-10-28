namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;

public record RemovePublicationReleaseDocumentsRequest
{
    public required string PublicationSlug { get; init; }
}
