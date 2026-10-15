#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;

/// <summary>
/// Writes a data set as a set of parquet files in the Public API's format.
/// </summary>
/// <remarks>
/// The indicators, filters and locations files are each written as soon as their data is known. Observations are
/// appended to a staging DuckDB database in the data set's directory, which allows an import to resume after a
/// restart. <see cref="WriteTimePeriodsAndData" /> then builds the time periods and data files from the staged
/// observations and the files already written, and removes the staging database.
/// </remarks>
public interface IParquetDataSetWriter
{
    bool HasIndicators(Guid subjectId);

    /// <summary>
    /// Writes the indicators file, replacing any written by an earlier attempt.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="indicators">All indicators for the data set, including their indicator groups.</param>
    /// <param name="cancellationToken"></param>
    Task WriteIndicators(
        Guid subjectId,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken = default
    );

    bool HasFilters(Guid subjectId);

    /// <summary>
    /// Writes the filters and filter options files, replacing any written by an earlier attempt.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="filters">All filters for the data set, including their groups and items.</param>
    /// <param name="cancellationToken"></param>
    Task WriteFilters(Guid subjectId, IReadOnlyList<Filter> filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the locations and location options files, replacing any written by an earlier attempt.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="locations">All locations for the data set.</param>
    /// <param name="cancellationToken"></param>
    Task WriteLocations(
        Guid subjectId,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Prepares the staging database for the data set's observations.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="filters">All filters for the data set.</param>
    /// <param name="indicators">All indicators for the data set.</param>
    /// <param name="resumeFromCsvRow">
    /// If null, any existing staging database is discarded and a new one is created. Otherwise, the existing
    /// staging database is kept and any observations staged from this CSV row onwards are removed, so that they
    /// can be appended again without creating duplicates.
    /// </param>
    /// <param name="cancellationToken"></param>
    Task StartStaging(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    );

    Task AppendObservations(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        IEnumerable<Observation> observations,
        CancellationToken cancellationToken = default
    );

    Task<long> CountStagedObservations(Guid subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the time periods and data files from the staged observations and the filter options and location
    /// options files, and removes the staging database.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="filters">All filters for the data set, as written by <see cref="WriteFilters" />.</param>
    /// <param name="indicators">All indicators for the data set, as written by <see cref="WriteIndicators" />.</param>
    /// <param name="locations">All locations for the data set, as written by <see cref="WriteLocations" />.</param>
    /// <param name="cancellationToken"></param>
    Task WriteTimePeriodsAndData(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    );
}
