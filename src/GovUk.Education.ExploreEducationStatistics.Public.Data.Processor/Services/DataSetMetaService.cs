using Dapper;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetMetaService(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IFilterRepository filterRepository,
    IIndicatorRepository indicatorRepository,
    ILocationRepository locationRepository,
    ITimePeriodRepository timePeriodRepository
) : IDataSetMetaService
{
    public async Task CreateDataSetVersionMeta(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await using var duckDb =
            DuckDbConnection.CreateFileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
        duckDb.Open();

        var csvDataFilePath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);

        var columns = duckDb.Query<(string ColumnName, string ColumnType)>(
                $"DESCRIBE SELECT * FROM '{csvDataFilePath}'"
            )
            .Select(row => row.ColumnName)
            .ToList();

        var allowedColumns = columns.ToHashSet();

        var metaFileRows = (await duckDb.QueryAsync<MetaFileRow>(
                new CommandDefinition(
                    $"SELECT * FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion)}'",
                    cancellationToken: cancellationToken
                )
            ))
            .ToList();

        await filterRepository.CreateFilterMetas(duckDb, dataSetVersion, allowedColumns, cancellationToken);
        await locationRepository.CreateLocationMetas(duckDb, dataSetVersion, allowedColumns, cancellationToken);
        await indicatorRepository.CreateIndicatorMetas(duckDb, dataSetVersion, allowedColumns, cancellationToken);
        var timePeriods =
            await timePeriodRepository.CreateTimePeriodMetas(duckDb, dataSetVersion, cancellationToken);
        var geographicLevels =
            await locationRepository.CreateGeographicLevelMeta(duckDb, dataSetVersion, cancellationToken);

        dataSetVersion.MetaSummary = BuildMetaSummary(timePeriods, metaFileRows, allowedColumns, geographicLevels);
        dataSetVersion.TotalResults = await duckDb.QuerySingleAsync<int>($"SELECT COUNT(*) FROM '{csvDataFilePath}'");

        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        await indicatorRepository.CreateIndicatorMetaTable(duckDb, dataSetVersion, cancellationToken);
        await locationRepository.CreateLocationMetaTable(duckDb, dataSetVersion, cancellationToken);
        await filterRepository.CreateFilterMetaTable(duckDb, dataSetVersion, cancellationToken);
        await timePeriodRepository.CreateTimePeriodMetaTable(duckDb, dataSetVersion, cancellationToken);
    }

    private static DataSetVersionMetaSummary BuildMetaSummary(
        IList<TimePeriodMeta> timePeriodMetas,
        IList<MetaFileRow> metaFileRows,
        HashSet<string> allowedColumns,
        GeographicLevelMeta geographicLevelMeta) =>
        new()
        {
            TimePeriodRange = new TimePeriodRange
            {
                Start = new TimePeriodRangeBound
                {
                    Period = timePeriodMetas[0].Period,
                    Code = timePeriodMetas[0].Code
                },
                End = new TimePeriodRangeBound
                {
                    Period = timePeriodMetas[^1].Period,
                    Code = timePeriodMetas[^1].Code
                },
            },
            Filters = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Filter
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .Select(row => row.Label)
                .ToList(),
            Indicators = metaFileRows
                .Where(
                    row => row.ColType == MetaFileRow.ColumnType.Indicator
                           && allowedColumns.Contains(row.ColName)
                )
                .OrderBy(row => row.Label)
                .Select(row => row.Label)
                .ToList(),
            GeographicLevels = geographicLevelMeta.Levels
        };
}
