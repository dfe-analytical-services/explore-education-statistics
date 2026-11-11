using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

public interface IContentApiClient
{
    /// <summary>
    /// Retrieve the latest release for the specified publication as a release document.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The latest release for the publication as a release document</returns>
    /// <exception cref="UnableToGetPublicationLatestReleaseDocumentException">Thrown if the call to the API was unsuccessful</exception>
    Task<ReleaseDocument> GetPublicationLatestReleaseDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// A simple call to check whether the Content API is available
    /// </summary>
    /// <returns>true if Content API responded, otherwise false and any error message</returns>
    Task<(bool WasSuccesssful, string? ErrorMessage)> Ping(CancellationToken cancellationToken = default);

    /// <summary>
    /// Given a Theme, get the Publications
    /// </summary>
    /// <param name="themeId">the theme id</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of information objects, one per live publication in the specified theme.</returns>
    /// <exception cref="GetPaginatedItemsException">Thrown if the call to the API was unsuccessful</exception>
    Task<PublicationInfo[]> GetPublicationsForTheme(Guid themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the published release id's for the specified publication.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns> An array of published release id's for the specified publication.</returns>
    /// <exception cref="UnableToGetPublicationReleaseIdsException">Thrown if the call to the API was unsuccessful</exception>
    Task<Guid[]> GetPublicationReleaseIds(string publicationSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a small amount of information for all live publications e.g. the publication id and slug
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of information objects, one per live publication.</returns>
    /// <exception cref="GetPaginatedItemsException">Thrown if the call to the API was unsuccessful</exception>
    Task<PublicationInfo[]> GetAllLivePublicationInfos(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve the latest published release version summary for the specified publication slug and release slug
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="releaseSlug">the release slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The latest published release version summary for the specified publication slug and release slug</returns>
    Task<ReleaseVersionSummary> GetReleaseVersionSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the keys of all the data set documents that are expected to exist. There is one per data file in the
    /// latest published version of each release.
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of data set document keys.</returns>
    /// <exception cref="UnableToGetDataSetDocumentKeysException">Thrown if the call to the API was unsuccessful</exception>
    Task<DataSetDocumentKey[]> GetDataSetDocumentKeys(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the data set documents for the latest published version of the specified release. There is one per
    /// data file.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="releaseSlug">the release slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of data set documents, which is empty if the release is not found.</returns>
    /// <exception cref="UnableToGetReleaseDataSetDocumentsException">Thrown if the call to the API was unsuccessful</exception>
    Task<DataSetDocument[]> GetReleaseDataSetDocuments(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the data set documents for the latest published version of every release of the specified
    /// publication. There is one per data file.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of data set documents, which is empty if the publication is not found.</returns>
    /// <exception cref="UnableToGetPublicationDataSetDocumentsException">Thrown if the call to the API was unsuccessful</exception>
    Task<DataSetDocument[]> GetPublicationDataSetDocuments(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );
}
