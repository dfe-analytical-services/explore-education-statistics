#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
    /// Observations at the given locations within the time period that have at least one of the given filter items
    /// for each filter, with their Location and FilterItems populated. If no arguments are provided, all observations
    /// for the data set are returned.
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
    /// <paramref name="includeItems" /> is true.
    /// </summary>
    Task<List<Filter>> ListFilters(bool includeItems = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicators for the data set restricted to the given ids, or all indicators for the data set if
    /// <paramref name="indicatorIds" /> is null.
    /// </summary>
    Task<List<Indicator>> ListIndicators(
        IEnumerable<Guid>? indicatorIds = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// All indicator groups for the data set, with their Indicators.
    /// </summary>
    Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default);

    /// <summary>
    /// Locations by id, or all locations if <paramref name="locationIds" /> is null. Unknown ids are ignored.
    /// </summary>
    Task<List<Location>> ListLocations(
        IEnumerable<Guid>? locationIds = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// The time periods across the data set's observations at the given locations, or across all the data
    /// set's observations if <paramref name="locationIds" /> is null, ordered by year and then time identifier.
    /// </summary>
    Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid>? locationIds = null,
        CancellationToken cancellationToken = default
    );
}
