#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

/// <summary>
/// Storage-agnostic read access to the content of a single data set (Subject): its observations, filters and
/// filter items, indicators, locations and time periods.
///
/// All methods are scoped to <see cref="SubjectId" />. Methods that accept a <see cref="FullTableQuery" />
/// require the query's SubjectId to match <see cref="SubjectId" />.
/// </summary>
public interface IStorageDataSet
{
    Guid SubjectId { get; }

    /// <summary>
    /// All observations matching the query, with their Location and FilterItems populated.
    /// </summary>
    Task<List<Observation>> ListObservations(FullTableQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams the observations matching the query in batches of at most <paramref name="batchSize" />.
    /// Observations are read lazily so that callers only need to hold a single batch in memory at any one time.
    /// Callers must not issue other reads against this data set until enumeration completes.
    /// </summary>
    IAsyncEnumerable<IReadOnlyList<Observation>> ListObservationBatches(
        FullTableQuery query,
        int batchSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Filter items (with their FilterGroup and Filter) that are present on the observations matching the
    /// query's location and time period selections.
    /// </summary>
    Task<List<FilterItem>> ListFilterItemsForQuery(FullTableQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filter items (with their FilterGroup and Filter) by id. Unknown ids are ignored.
    /// </summary>
    Task<List<FilterItem>> ListFilterItems(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts of the given filter items grouped by the id of the Filter they belong to.
    /// Throws <see cref="ArgumentException" /> if any of the ids cannot be found.
    /// </summary>
    Task<Dictionary<Guid, int>> CountFilterItemsByFilter(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// All filters for the data set, with their FilterGroups and FilterItems.
    /// </summary>
    Task<List<Filter>> ListFilters(CancellationToken cancellationToken = default);

    /// <summary>
    /// All indicators for the data set.
    /// </summary>
    Task<List<Indicator>> ListIndicators(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicators for the data set restricted to the given ids. Ids belonging to other data sets are excluded.
    /// </summary>
    Task<List<Indicator>> ListIndicators(IEnumerable<Guid> indicatorIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// All indicator groups for the data set, with their Indicators.
    /// </summary>
    Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default);

    /// <summary>
    /// The distinct locations referenced by the data set's observations.
    /// </summary>
    Task<List<Location>> ListLocations(CancellationToken cancellationToken = default);

    /// <summary>
    /// Locations by id. Unknown ids are ignored.
    ///
    /// This is not restricted to the data set: locations are shared and immutable, and a permalink may reference
    /// locations that are no longer linked to any data set.
    /// </summary>
    Task<List<Location>> ListLocations(IEnumerable<Guid> locationIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// The distinct time periods across the data set's observations, ordered by year and then time identifier.
    /// </summary>
    Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// As <see cref="ListTimePeriods(CancellationToken)" />, but only over the observations at the given locations.
    /// </summary>
    Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    );
}
