namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public record RemovePublicationSearchableDocumentsRequest
{
    public required string PublicationSlug { get; init; }
}
