namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public record CreatePublicationLatestReleaseSearchableDocumentRequest
{
    public required string PublicationSlug { get; init; }
}
