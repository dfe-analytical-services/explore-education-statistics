#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IFilterItemRepository
{
    Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds);

    /// <summary>
    /// Method to retrieve a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
    /// <see cref="Filter" />s that are present on a set of previously-matched <see cref="MatchedObservation" />s.
    ///
    /// The "#MatchedObservation" temporary table should already be available in the current database session.
    ///
    /// Note that this method is for use when a set of matched Observations have had their ids stored in the
    /// #MatchedObservation temporary table.
    /// </summary>
    ///
    /// <param name="subjectId">The Subject ID that the Filter Items belong to.</param>
    /// <param name="matchedObservationsTableReference">A reference to the temp table with
    /// the already-inserted ObservationIds that match the current query.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    ///
    /// <returns>A set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
    /// <see cref="Filter" />s that appear in the already-matched <see cref="Observation"/>s.
    /// </returns>
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Method to retrieve a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
    /// <see cref="Filter" />s that are present on a given set of Observations.
    ///
    /// Note that this method is for use when there is a set of Observations that has been fetched into memory
    /// already, as it is optimised for this scenario.
    /// </summary>
    ///
    /// <param name="observations">A set of Observations that have been already fetched into memory.</param>
    /// <returns>a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
    /// <see cref="Filter" />s</returns>
    Task<IList<FilterItem>> GetFilterItemsFromObservations(IEnumerable<Observation> observations);
}
