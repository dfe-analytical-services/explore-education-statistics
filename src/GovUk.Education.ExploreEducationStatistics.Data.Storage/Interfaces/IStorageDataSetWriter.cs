#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

/// <summary>
/// Storage-agnostic methods used to write, or to support the writing of, a single data set's data while it is being
/// imported. Reads of the imported data set go through <see cref="DataSet" />.
///
/// All methods are scoped to <see cref="SubjectId" />.
/// </summary>
public interface IStorageDataSetWriter
{
    Guid SubjectId { get; }

    /// <summary>
    /// The data set being written to. Its data may only be readable once <see cref="Complete" /> has run, so reads
    /// made while importing go through this writer instead.
    /// </summary>
    IStorageDataSet DataSet { get; }

    /// <summary>
    /// Whether <see cref="WriteIndicators" /> has already run, e.g. in an earlier attempt at the import.
    /// </summary>
    Task<bool> HasIndicators(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all of the data set's indicators together with their IndicatorGroups.
    /// </summary>
    Task WriteIndicators(IReadOnlyList<Indicator> indicators, CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether <see cref="WriteFilters" /> has already run, e.g. in an earlier attempt at the import.
    /// </summary>
    Task<bool> HasFilters(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all of the data set's filters together with their FilterGroups, FilterItems and auto select filter
    /// items.
    /// </summary>
    Task WriteFilters(IReadOnlyList<Filter> filters, CancellationToken cancellationToken = default);

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
    /// The filters written by <see cref="WriteFilters" />, with their FilterGroups and FilterItems if
    /// <paramref name="includeItems" /> is true. Once imported, read them from <see cref="DataSet" /> instead.
    /// </summary>
    Task<List<Filter>> ListFilters(bool includeItems = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// The IndicatorGroups written by <see cref="WriteIndicators" />, with their Indicators. Once
    /// imported, read them from <see cref="DataSet" /> instead.
    /// </summary>
    Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default);

    /// <summary>
    /// Distinct pairs of filter items from the given parent and child filters that occur together on the same
    /// observation, used to generate the data set's FilterHierarchies. Once imported, read them from there instead.
    /// </summary>
    Task<List<(Guid ParentFilterItemId, Guid ChildFilterItemId)>> ListFilterItemRelationships(
        Guid parentFilterId,
        Guid childFilterId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// The geographic levels of the locations referenced by the data set's observations, used to populate the data
    /// set's DataSetFileVersionGeographicLevels. Once imported, read them from there instead.
    /// </summary>
    Task<List<GeographicLevel>> ListGeographicLevels(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finishes writing the data set once all of its observations have been written. Can be run again if an
    /// earlier attempt was interrupted, until <see cref="CleanUp" /> is called.
    /// </summary>
    Task Complete(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes anything kept so that <see cref="Complete" /> could be run again, once the data set has been
    /// recorded as complete.
    /// </summary>
    Task CleanUp(CancellationToken cancellationToken = default);
}
