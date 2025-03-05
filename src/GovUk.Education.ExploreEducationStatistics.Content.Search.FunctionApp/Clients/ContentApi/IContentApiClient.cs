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
    Task<ReleaseSearchableDocument> GetPublicationLatestReleaseSearchableDocumentAsync(
        string publicationSlug,
        CancellationToken cancellationToken = default);
}
