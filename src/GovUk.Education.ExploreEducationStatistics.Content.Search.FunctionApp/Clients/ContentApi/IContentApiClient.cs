using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

public interface IContentApiClient
{
    /// <summary>
    /// Retrieve the latest release for the specified publication in a searchable document format.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The latest release for the publication in searchable document format</returns>
    /// <exception cref="UnableToGetPublicationLatestReleaseSearchViewModelException">Thrown if the call to the API was unsuccessful</exception>
    Task<ReleaseSearchableDocument> GetPublicationLatestReleaseSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// A simple call to check whether the Content API is available
    /// </summary>
    /// <returns>true if Content API responded, otherwise false and any error message</returns>
    Task<(bool WasSuccesssful, string? ErrorMessage)> Ping(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Given a Theme, get the Publications
    /// </summary>
    /// <param name="themeId">the theme id</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of information objects, one per live publication in the specified theme.</returns>
    /// <exception cref="GetPaginatedItemsException">Thrown if the call to the API was unsuccessful</exception>
    Task<PublicationInfo[]> GetPublicationsForTheme(
        Guid themeId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the releases for the specified publication.
    /// </summary>
    /// <param name="publicationSlug">the publication slug</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns> An array of releases for the specified publication.</returns>
    /// <exception cref="UnableToGetReleasesForPublicationException">Thrown if the call to the API was unsuccessful</exception>
    Task<ReleaseInfo[]> GetReleasesForPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a small amount of information for all live publications e.g. the publication id and slug
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>An array of information objects, one per live publication.</returns>
    /// <exception cref="GetPaginatedItemsException">Thrown if the call to the API was unsuccessful</exception>
    Task<PublicationInfo[]> GetAllLivePublicationInfos(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieve the Release Summary for the specified publication slug and release slug
    /// </summary>
    Task<ReleaseSummary> GetReleaseSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );
}
