﻿namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;

public interface ISearchableDocumentRemover
{
    Task<RemovePublicationSearchableDocumentsResponse> RemovePublicationSearchableDocuments(
        RemovePublicationSearchableDocumentsRequest request,
        CancellationToken cancellationToken = default);
    
    Task<RemoveSearchableDocumentResponse> RemoveSearchableDocument(
        RemoveSearchableDocumentRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveAllSearchableDocuments(CancellationToken cancellationToken);
}
