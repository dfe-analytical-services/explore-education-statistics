#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

/// <summary>
/// Writes a data set's indicators, filters and locations as parquet files as soon as each is known, and stages its
/// observations so that the time periods and data files can be written once the import is complete.
/// </summary>
public class ParquetV1DataSetWriter(
    Guid subjectId,
    ParquetV1DataSetFactory parquetV1DataSetFactory,
    IParquetDataSetWriter parquetDataSetWriter,
    IGuidGenerator guidGenerator
) : IStorageDataSetWriter
{
    private readonly ParquetV1DataSet _parquetV1DataSet = parquetV1DataSetFactory.Create(subjectId);

    private IReadOnlyList<Filter>? _filters;
    private IReadOnlyList<Indicator>? _indicators;
    private Dictionary<string, Guid>? _locationIdsByKey;

    public Guid SubjectId { get; } = subjectId;

    public IStorageDataSet DataSet => _parquetV1DataSet;

    public Task<bool> HasIndicators(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(parquetDataSetWriter.HasIndicators(SubjectId));
    }

    public Task WriteIndicators(IReadOnlyList<Indicator> indicators, CancellationToken cancellationToken = default)
    {
        return parquetDataSetWriter.WriteIndicators(SubjectId, indicators, cancellationToken);
    }

    public Task<bool> HasFilters(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(parquetDataSetWriter.HasFilters(SubjectId));
    }

    public Task WriteFilters(IReadOnlyList<Filter> filters, CancellationToken cancellationToken = default)
    {
        return parquetDataSetWriter.WriteFilters(SubjectId, filters, cancellationToken);
    }

    public Task WriteLocations(IReadOnlyCollection<Location> locations, CancellationToken cancellationToken = default)
    {
        foreach (var location in locations.Where(location => location.Id == Guid.Empty))
        {
            location.Id = guidGenerator.NewGuid();
        }

        return parquetDataSetWriter.WriteLocations(SubjectId, locations.ToList(), cancellationToken);
    }

    public Guid GetLocationId(Location location)
    {
        if (_locationIdsByKey is null)
        {
            throw new InvalidOperationException($"{nameof(StartObservations)} must be called first");
        }

        return _locationIdsByKey[ImporterLocationCache.GetLocationCacheKey(location)];
    }

    public async Task StartObservations(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    )
    {
        _filters = filters;
        _indicators = indicators;

        _locationIdsByKey = (await _parquetV1DataSet.ListLocations(cancellationToken: cancellationToken)).ToDictionary(
            ImporterLocationCache.GetLocationCacheKey,
            location => location.Id
        );

        await parquetDataSetWriter.StartStaging(SubjectId, filters, indicators, resumeFromCsvRow, cancellationToken);
    }

    public Task AppendObservations(IReadOnlyList<Observation> observations)
    {
        if (_filters is null || _indicators is null)
        {
            throw new InvalidOperationException($"{nameof(StartObservations)} must be called first");
        }

        return parquetDataSetWriter.AppendObservations(SubjectId, _filters, _indicators, observations);
    }

    public Task<long> CountObservations(CancellationToken cancellationToken = default)
    {
        return parquetDataSetWriter.CountStagedObservations(SubjectId, cancellationToken);
    }

    public Task<List<(Guid ParentFilterItemId, Guid ChildFilterItemId)>> ListFilterItemRelationships(
        Guid parentFilterId,
        Guid childFilterId,
        CancellationToken cancellationToken = default
    )
    {
        return _parquetV1DataSet.ListFilterItemRelationships(parentFilterId, childFilterId, cancellationToken);
    }

    public Task<List<GeographicLevel>> ListGeographicLevels(CancellationToken cancellationToken = default)
    {
        return _parquetV1DataSet.ListGeographicLevels(cancellationToken);
    }

    public async Task Complete(CancellationToken cancellationToken = default)
    {
        await parquetDataSetWriter.WriteTimePeriodsAndData(
            SubjectId,
            await _parquetV1DataSet.ListFilters(cancellationToken: cancellationToken),
            await _parquetV1DataSet.ListIndicators(cancellationToken: cancellationToken),
            await _parquetV1DataSet.ListLocations(cancellationToken: cancellationToken),
            cancellationToken
        );
    }
}
