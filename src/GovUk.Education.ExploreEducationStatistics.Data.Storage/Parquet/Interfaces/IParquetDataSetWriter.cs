#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;

/// <summary>
/// Writes a data set as a set of parquet files in the Public API's format.
/// </summary>
/// <remarks>
/// Observations are first appended to a staging DuckDB database in the data set's directory, which allows an
/// import to resume after a restart. <see cref="WriteParquetFiles" /> then builds each table from the staged
/// observations and the data set's meta, exports them as parquet files and removes the staging database.
/// </remarks>
public interface IParquetDataSetWriter
{
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

    Task<List<Guid>> ListStagedLocationIds(Guid subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the parquet files from the staged observations and removes the staging database.
    /// </summary>
    /// <param name="subjectId">The data set's subject id.</param>
    /// <param name="filters">All filters for the data set, including their groups and items.</param>
    /// <param name="indicatorGroups">All indicator groups for the data set, including their indicators.</param>
    /// <param name="locations">The locations of the staged observations.</param>
    /// <param name="cancellationToken"></param>
    Task WriteParquetFiles(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<IndicatorGroup> indicatorGroups,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    );
}
