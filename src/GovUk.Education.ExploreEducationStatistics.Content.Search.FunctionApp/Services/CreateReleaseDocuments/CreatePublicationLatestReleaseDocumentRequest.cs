namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateReleaseDocuments;

public record CreatePublicationLatestReleaseDocumentRequest
{
    public required string PublicationSlug { get; init; }
}
