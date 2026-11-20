namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

public interface IDataSetDocumentCreator
{
    /// <summary>
    /// Create or update the data set documents of a release.
    /// </summary>
    Task<CreateDataSetDocumentsResponse> CreateReleaseDataSetDocuments(
        CreateReleaseDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create or update the data set documents of every release of a publication.
    /// </summary>
    Task<CreateDataSetDocumentsResponse> CreatePublicationDataSetDocuments(
        CreatePublicationDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    );
}
