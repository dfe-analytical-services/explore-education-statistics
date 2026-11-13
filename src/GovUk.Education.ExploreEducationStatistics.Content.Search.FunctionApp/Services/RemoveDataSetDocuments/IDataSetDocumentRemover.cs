namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;

public interface IDataSetDocumentRemover
{
    /// <summary>
    /// Remove the data set documents of a publication, of one of its releases, or of a single data file, depending on
    /// which ids are specified in the request.
    /// </summary>
    Task<RemoveDataSetDocumentsResponse> RemoveDataSetDocuments(
        RemoveDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Remove the data set documents of every publication.
    /// </summary>
    Task<RemoveDataSetDocumentsResponse> RemoveAllDataSetDocuments(CancellationToken cancellationToken = default);
}
