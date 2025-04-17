namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public interface ISearchableDocumentRemover
{
    Task<RemovePublicationSearchableDocumentsResponse> RemovePublicationSearchableDocuments(
        RemovePublicationSearchableDocumentsRequest request,
        CancellationToken cancellationToken = default);
}
