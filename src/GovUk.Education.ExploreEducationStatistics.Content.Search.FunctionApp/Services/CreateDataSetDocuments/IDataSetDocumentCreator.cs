namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

public interface IDataSetDocumentCreator
{
    /// <summary>
    /// Create or update the data set documents of a release, and remove any that it should no longer have.
    /// </summary>
    Task<RefreshDataSetDocumentsResponse> RefreshReleaseDataSetDocuments(
        RefreshReleaseDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create or update the data set documents of every release of a publication, and remove any that it should no
    /// longer have.
    /// </summary>
    Task<RefreshDataSetDocumentsResponse> RefreshPublicationDataSetDocuments(
        RefreshPublicationDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    );
}
