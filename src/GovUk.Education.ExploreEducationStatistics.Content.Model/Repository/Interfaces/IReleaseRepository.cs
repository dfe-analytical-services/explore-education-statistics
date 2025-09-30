#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseRepository
{
    /// <summary>
    /// Retrieves the published releases in release series order that are associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the published releases in release series order associated with the publication.</returns>
    Task<List<Release>> ListPublishedReleases(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the id's of all published releases that are associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the id's of all published releases associated with the publication.</returns>
    Task<List<Guid>> ListPublishedReleaseIds(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );
}
