namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;

public interface IReleaseDocumentRemover
{
    Task<RemovePublicationReleaseDocumentsResponse> RemovePublicationReleaseDocuments(
        RemovePublicationReleaseDocumentsRequest request,
        CancellationToken cancellationToken = default
    );

    Task<RemoveReleaseDocumentResponse> RemoveReleaseDocument(
        RemoveReleaseDocumentRequest request,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllReleaseDocuments(CancellationToken cancellationToken);
}
