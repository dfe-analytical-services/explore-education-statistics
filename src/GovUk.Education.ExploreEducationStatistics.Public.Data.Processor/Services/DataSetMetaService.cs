using Dapper;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetMetaService(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IFilterMetaRepository filterMetaRepository,
    IIndicatorMetaRepository indicatorMetaRepository,
    IGeographicLevelMetaRepository geographicLevelMetaRepository,
    ILocationMetaRepository locationMetaRepository,
    ITimePeriodMetaRepository timePeriodMetaRepository,
    IFilterOptionsDuckDbRepository filterOptionsDuckDbRepository,
    IIndicatorsDuckDbRepository indicatorsDuckDbRepository,
    ILocationsDuckDbRepository locationsDuckDbRepository,
    ITimePeriodsDuckDbRepository timePeriodsDuckDbRepository
) : IDataSetMetaService
{
    public async Task CreateDataSetVersionMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await publicDataDbContext.DataSetVersions
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);

        await using var duckDbConnection =
            DuckDbConnection.CreateFileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
        duckDbConnection.Open();

        var columns = (await duckDbConnection.SqlBuilder(
                    $"DESCRIBE SELECT * FROM '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}'")
                .QueryAsync<(string ColumnName, string ColumnType)>(cancellationToken: cancellationToken))
            .Select(row => row.ColumnName)
            .ToList();

        var allowedColumns = columns.ToHashSet();

        var metaFileRows = (await duckDbConnection.SqlBuilder(
                $"SELECT * FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'")
            .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken)).AsList();

        await filterMetaRepository.CreateFilterMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        await indicatorMetaRepository.CreateIndicatorMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        var geographicLevelMeta = await geographicLevelMetaRepository.CreateGeographicLevelMeta(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);

        await locationMetaRepository.CreateLocationMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        var timePeriodMetas = await timePeriodMetaRepository.CreateTimePeriodMetas(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);

        dataSetVersion.MetaSummary =
            BuildMetaSummary(timePeriodMetas, metaFileRows, allowedColumns, geographicLevelMeta);
        dataSetVersion.TotalResults = await CountCsvRows(duckDbConnection, dataSetVersion, cancellationToken);

        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        await indicatorsDuckDbRepository.CreateIndicatorsTable(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);

        await locationsDuckDbRepository.CreateLocationsTable(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);

        await filterOptionsDuckDbRepository.CreateFilterOptionsTable(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);

        await timePeriodsDuckDbRepository.CreateTimePeriodsTable(
            duckDbConnection,
            dataSetVersion,
            cancellationToken);
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

    private async Task<int> CountCsvRows(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await duckDbConnection.SqlBuilder(
            $"""
             SELECT COUNT(*)
             FROM '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}'
             """
        ).QuerySingleAsync<int>(cancellationToken: cancellationToken);
    }
}