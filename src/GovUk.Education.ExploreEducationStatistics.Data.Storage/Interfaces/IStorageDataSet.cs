#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

/// <summary>
/// Storage-agnostic read access to a single data set: its observations, filters, indicators, locations and time periods
///
/// All methods are scoped to <see cref="SubjectId" />.
/// </summary>
public interface IStorageDataSet
{
    Guid SubjectId { get; }

    /// <summary>
    /// Observations at the given locations within the time period that have every one of the given filter items,
    /// with their Location and FilterItems populated.
    /// </summary>
    Task<List<Observation>> ListObservations(
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// As <see cref="ListObservations" />, but streamed in batches of at most <paramref name="batchSize" />.
    /// Observations are read lazily so that callers only need to hold a single batch in memory at any one time.
    /// Callers must not issue other reads against this data set until enumeration completes.
    /// </summary>
    IAsyncEnumerable<IReadOnlyList<Observation>> ListObservationBatches(
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        int batchSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Filter items (with their FilterGroup and Filter) that are present on the observations at the given
    /// locations within the time period.
    /// </summary>
    Task<List<FilterItem>> ListFilterItemsForQuery(
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Filter items (with their FilterGroup and Filter) for the data set restricted to the given ids.
    /// Ids that are unknown or belong to other data sets are ignored.
    /// </summary>
    Task<List<FilterItem>> ListFilterItems(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// All filters for the data set. Their FilterGroups and FilterItems are only populated when
    /// <paramref name="includeItems" /> is true, as filter items can be numerous.
    /// </summary>
    Task<List<Filter>> ListFilters(bool includeItems = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Distinct pairs of filter items from the given parent and child filters that occur together on the same
    /// observation. Filters belonging to other data sets yield no pairs.
    /// </summary>
    Task<List<(Guid ParentFilterItemId, Guid ChildFilterItemId)>> ListFilterItemRelationships(
        Guid parentFilterId,
        Guid childFilterId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// All indicators for the data set.
    /// </summary>
    Task<List<Indicator>> ListIndicators(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicators for the data set restricted to the given ids. Ids belonging to other data sets are excluded,
    /// and an empty set of ids yields no indicators.
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
    /// Locations by id. Unknown ids are ignored. `locationIds` are expected to belong to the data set, but might be
    /// returned even if they belong to a different data set.
    /// </summary>
    Task<List<Location>> ListLocations(IEnumerable<Guid> locationIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// The distinct geographic levels of the locations referenced by the data set's observations.
    /// </summary>
    Task<List<GeographicLevel>> ListGeographicLevels(CancellationToken cancellationToken = default);

    /// <summary>
    /// The distinct time periods across the data set's observations, ordered by year and then time identifier.
    /// </summary>
    Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// As <see cref="ListTimePeriods(CancellationToken)" />, but only over the observations at the given locations.
    /// Location ids not referenced by the data set's observations are ignored.
    /// </summary>
    Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    );
}
