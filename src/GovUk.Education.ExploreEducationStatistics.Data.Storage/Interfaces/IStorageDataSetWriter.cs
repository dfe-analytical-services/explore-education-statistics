#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

/// <summary>
/// Storage-agnostic write access to a single data set while it is being imported: its filters, indicators,
/// locations and observations.
///
/// All methods are scoped to <see cref="SubjectId" />.
/// </summary>
public interface IStorageDataSetWriter
{
    Guid SubjectId { get; }

    /// <summary>
    /// Read access to what has been written so far, for use while the import is in progress.
    /// </summary>
    IStorageDataSet DataSet { get; }

    /// <summary>
    /// Writes the data set's filters, and its indicators together with their IndicatorGroups.
    /// </summary>
    Task WriteFiltersAndIndicators(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Writes filter items together with their FilterGroups, and sets the auto select filter item of each filter in
    /// <paramref name="autoSelectFilterItemsByFilterId" />, clearing it where the filter has none.
    /// </summary>
    Task WriteFilterItems(
        IReadOnlyList<FilterItem> filterItems,
        IReadOnlyDictionary<Guid, FilterItem?> autoSelectFilterItemsByFilterId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Writes those of the given locations that have not been written already.
    /// </summary>
    Task WriteLocations(IReadOnlyCollection<Location> locations, CancellationToken cancellationToken = default);

    /// <summary>
    /// The id of the written location matching the given one, which must already have been written by
    /// <see cref="WriteLocations" />.
    /// </summary>
    Guid GetLocationId(Location location);

    /// <summary>
    /// Prepares for observations to be written by <see cref="WriteObservations" />.
    /// </summary>
    /// <param name="filters">All filters for the data set.</param>
    /// <param name="indicators">All indicators for the data set.</param>
    /// <param name="resumeFromCsvRow">
    /// Null if no observations have been written yet. Otherwise, the CSV row that the import is resuming from, so
    /// that any observations already written from this row onwards can be discarded and written again.
    /// </param>
    /// <param name="cancellationToken"></param>
    Task StartObservations(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Writes a batch of observations together with their ObservationFilterItems.
    /// </summary>
    Task WriteObservations(IReadOnlyList<Observation> observations, CancellationToken cancellationToken = default);

    /// <summary>
    /// The number of observations written.
    /// </summary>
    Task<long> CountObservations(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finishes writing the data set once all of its observations have been written.
    /// </summary>
    Task Complete(CancellationToken cancellationToken = default);
}
