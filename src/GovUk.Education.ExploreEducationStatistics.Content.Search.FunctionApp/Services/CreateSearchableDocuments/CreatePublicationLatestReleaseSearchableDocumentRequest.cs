namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;

public record CreatePublicationLatestReleaseSearchableDocumentRequest
{
    public required string PublicationSlug { get; init; }
}
