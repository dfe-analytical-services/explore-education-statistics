#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

/// <summary>
/// Storage-agnostic methods used to write, or to support the writing of, a single data set's data while it is being
/// imported. Other reads go through <see cref="IStorageDataSet" />.
///
/// All methods are scoped to <see cref="SubjectId" />.
/// </summary>
public interface IStorageDataSetWriter
{
    Guid SubjectId { get; }

    /// <summary>
    /// Writes the data set's filters (not their groups and items), and its indicators together with their
    /// IndicatorGroups.
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
    /// Writes a batch of observations together with their ObservationFilterItems.
    /// </summary>
    Task WriteObservations(IReadOnlyList<Observation> observations, CancellationToken cancellationToken = default);

    /// <summary>
    /// The number of observations in the data set.
    /// </summary>
    Task<long> CountObservations(CancellationToken cancellationToken = default);

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
}
