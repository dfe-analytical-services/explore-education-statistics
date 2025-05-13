namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;

public record RemovePublicationSearchableDocumentsRequest
{
    public required string PublicationSlug { get; init; }
}
