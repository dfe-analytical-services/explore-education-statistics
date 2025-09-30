namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;

public record RemovePublicationSearchableDocumentsResponse(
    IReadOnlyDictionary<Guid, bool> ReleaseIdToDeletionResult
);
