using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
    public async Task<DataSetVersionMappingMeta> ReadDataSetVersionMappingMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetVersion = await GetDataSetVersion(dataSetVersionId, cancellationToken);

        await using var duckDbConnection = DuckDbConnection.CreateFileConnection(
            dataSetVersionPathResolver.DuckDbPath(dataSetVersion)
        );
        duckDbConnection.Open();

        var allowedColumns = await GetAllowedColumns(duckDbConnection, dataSetVersion, cancellationToken);

        var metaFileRows = await GetMetaFileRows(duckDbConnection, dataSetVersion, cancellationToken);

        var filterMetas = await filterMetaRepository.ReadFilterMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken
        );

        var geographicLevelMeta = await geographicLevelMetaRepository.ReadGeographicLevelMeta(
            duckDbConnection,
            dataSetVersion,
            cancellationToken
        );

        var indicatorMetas = await indicatorMetaRepository.ReadIndicatorMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken
        );

        var locationMetas = await locationMetaRepository.ReadLocationMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken
        );

        var timePeriodMetas = await timePeriodMetaRepository.ReadTimePeriodMetas(
            duckDbConnection,
            dataSetVersion,
            cancellationToken
        );

        var metaSummary = BuildMetaSummary(timePeriodMetas, metaFileRows, allowedColumns, geographicLevelMeta);

        return new DataSetVersionMappingMeta
        {
            Filters = filterMetas,
            Locations = locationMetas,
            MetaSummary = metaSummary,
            Indicators = indicatorMetas,
            GeographicLevel = geographicLevelMeta,
            TimePeriods = timePeriodMetas,
        };
    }

    public async Task CreateDataSetVersionMeta(Guid dataSetVersionId, CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await GetDataSetVersion(dataSetVersionId, cancellationToken);

        await using var duckDbConnection = DuckDbConnection.CreateFileConnection(
            dataSetVersionPathResolver.DuckDbPath(dataSetVersion)
        );
        duckDbConnection.Open();

        var allowedColumns = await GetAllowedColumns(duckDbConnection, dataSetVersion, cancellationToken);

        var metaFileRows = await GetMetaFileRows(duckDbConnection, dataSetVersion, cancellationToken);

        await publicDataDbContext.RequireTransaction(async () =>
        {
            await filterMetaRepository.CreateFilterMetas(
                duckDbConnection,
                dataSetVersion,
                allowedColumns,
                cancellationToken
            );

            await indicatorMetaRepository.CreateIndicatorMetas(
                duckDbConnection,
                dataSetVersion,
                allowedColumns,
                cancellationToken
            );

            var geographicLevelMeta = await geographicLevelMetaRepository.CreateGeographicLevelMeta(
                duckDbConnection,
                dataSetVersion,
                cancellationToken
            );

            await locationMetaRepository.CreateLocationMetas(
                duckDbConnection,
                dataSetVersion,
                allowedColumns,
                cancellationToken
            );

            var timePeriodMetas = await timePeriodMetaRepository.CreateTimePeriodMetas(
                duckDbConnection,
                dataSetVersion,
                cancellationToken
            );

            dataSetVersion.MetaSummary = BuildMetaSummary(
                timePeriodMetas,
                metaFileRows,
                allowedColumns,
                geographicLevelMeta
            );

            dataSetVersion.TotalResults = await CountCsvRows(duckDbConnection, dataSetVersion, cancellationToken);

            await publicDataDbContext.SaveChangesAsync(cancellationToken);

            await indicatorsDuckDbRepository.CreateIndicatorsTable(duckDbConnection, dataSetVersion, cancellationToken);

            await locationsDuckDbRepository.CreateLocationsTable(duckDbConnection, dataSetVersion, cancellationToken);

            await filterOptionsDuckDbRepository.CreateFilterOptionsTable(
                duckDbConnection,
                dataSetVersion,
                cancellationToken
            );

            await timePeriodsDuckDbRepository.CreateTimePeriodsTable(
                duckDbConnection,
                dataSetVersion,
                cancellationToken
            );
        });
    }

    private async Task<List<MetaFileRow>> GetMetaFileRows(
        DuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await duckDbConnection
                .SqlBuilder($"SELECT * FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'")
                .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken)
        ).AsList();
    }

    private async Task<HashSet<string>> GetAllowedColumns(
        DuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var columns = (
            await duckDbConnection
                .SqlBuilder(
                    $"""
                    DESCRIBE SELECT *
                    FROM read_csv(
                        '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}',
                        {DuckDbConstants.ReadCsvOptions:raw}
                    )
                    """
                )
                .QueryAsync<(string ColumnName, string ColumnType)>(cancellationToken: cancellationToken)
        ).Select(row => row.ColumnName).ToList();

        return [.. columns];
    }

    private async Task<DataSetVersion> GetDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext.DataSetVersions.SingleAsync(
            dsv => dsv.Id == dataSetVersionId,
            cancellationToken
        );
    }

    private static DataSetVersionMetaSummary BuildMetaSummary(
        IList<TimePeriodMeta> timePeriodMetas,
        IList<MetaFileRow> metaFileRows,
        HashSet<string> allowedColumns,
        GeographicLevelMeta geographicLevelMeta
    )
    {
        return new()
        {
            TimePeriodRange = new TimePeriodRange
            {
                Start = new TimePeriodRangeBound { Period = timePeriodMetas[0].Period, Code = timePeriodMetas[0].Code },
                End = new TimePeriodRangeBound { Period = timePeriodMetas[^1].Period, Code = timePeriodMetas[^1].Code },
            },
            Filters = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Filter && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .Select(row => row.Label)
                .ToList(),
            Indicators = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .Select(row => row.Label)
                .ToList(),
            GeographicLevels = geographicLevelMeta.Levels,
        };
    }

    private async Task<int> CountCsvRows(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        return await duckDbConnection
            .SqlBuilder(
                $"""
                SELECT COUNT(*)
                FROM read_csv(
                    '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}',
                    {DuckDbConstants.ReadCsvOptions:raw}
                )   
                """
            )
            .QuerySingleAsync<int>(cancellationToken: cancellationToken);
    }
}
