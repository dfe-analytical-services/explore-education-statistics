#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

/// <summary>
/// Writes a data set's meta to StatsDB, and stages its observations so that they can be written as parquet files
/// together with that meta once the import is complete.
/// </summary>
public class ParquetV1DataSetWriter(
    Guid subjectId,
    StatisticsDbDataSetWriterFactory statisticsDbDataSetWriterFactory,
    IParquetDataSetWriter parquetDataSetWriter
) : IStorageDataSetWriter
{
    // Keeps the number of query parameters within SQL Server's limit.
    private const int LocationQueryBatchSize = 1_000;

    private readonly StatisticsDbDataSetWriter _statisticsDbDataSetWriter = statisticsDbDataSetWriterFactory.Create(
        subjectId
    );

    private IReadOnlyList<Filter>? _filters;
    private IReadOnlyList<Indicator>? _indicators;

    public Guid SubjectId { get; } = subjectId;

    public IStorageDataSet DataSet => _statisticsDbDataSetWriter.DataSet;

    public Task WriteFiltersAndIndicators(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken = default
    )
    {
        return _statisticsDbDataSetWriter.WriteFiltersAndIndicators(filters, indicators, cancellationToken);
    }

    public Task WriteFilterItems(
        IReadOnlyList<FilterItem> filterItems,
        IReadOnlyDictionary<Guid, FilterItem?> autoSelectFilterItemsByFilterId,
        CancellationToken cancellationToken = default
    )
    {
        return _statisticsDbDataSetWriter.WriteFilterItems(
            filterItems,
            autoSelectFilterItemsByFilterId,
            cancellationToken
        );
    }

    public Task WriteLocations(IReadOnlyCollection<Location> locations, CancellationToken cancellationToken = default)
    {
        return _statisticsDbDataSetWriter.WriteLocations(locations, cancellationToken);
    }

    public Guid GetLocationId(Location location)
    {
        return _statisticsDbDataSetWriter.GetLocationId(location);
    }

    public Task StartObservations(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    )
    {
        _filters = filters;
        _indicators = indicators;

        return parquetDataSetWriter.StartStaging(SubjectId, filters, indicators, resumeFromCsvRow, cancellationToken);
    }

    public Task WriteObservations(
        IReadOnlyList<Observation> observations,
        CancellationToken cancellationToken = default
    )
    {
        if (_filters is null || _indicators is null)
        {
            throw new InvalidOperationException($"{nameof(StartObservations)} must be called first");
        }

        return parquetDataSetWriter.AppendObservations(
            SubjectId,
            _filters,
            _indicators,
            observations,
            cancellationToken
        );
    }

    public Task<long> CountObservations(CancellationToken cancellationToken = default)
    {
        return parquetDataSetWriter.CountStagedObservations(SubjectId, cancellationToken);
    }

    public async Task Complete(CancellationToken cancellationToken = default)
    {
        var filters = await DataSet.ListFilters(includeItems: true, cancellationToken);
        var indicatorGroups = await DataSet.ListIndicatorGroups(cancellationToken);

        var locationIds = await parquetDataSetWriter.ListStagedLocationIds(SubjectId, cancellationToken);
        var locations = new List<Location>();

        foreach (var locationIdsBatch in locationIds.Chunk(LocationQueryBatchSize))
        {
            locations.AddRange(await DataSet.ListLocations(locationIdsBatch, cancellationToken));
        }

        await parquetDataSetWriter.WriteParquetFiles(SubjectId, filters, indicatorGroups, locations, cancellationToken);
    }
}
