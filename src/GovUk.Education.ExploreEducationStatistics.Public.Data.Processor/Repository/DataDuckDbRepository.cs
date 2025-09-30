using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class DataDuckDbRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IDataDuckDbRepository
{
    public async Task CreateDataTable(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.FilterMetas)
            .Include(dsv => dsv.IndicatorMetas)
            .Include(dsv => dsv.LocationMetas)
            .Include(dsv => dsv.TimePeriodMetas)
            .AsSplitQuery()
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);

        await using var duckDbConnection =
            DuckDbConnection.CreateFileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));

        await duckDbConnection.SqlBuilder("CREATE SEQUENCE data_seq START 1")
            .ExecuteAsync(cancellationToken: cancellationToken);

        string[] columns =
        [
            $"{DataTable.Cols.Id} UINTEGER NOT NULL PRIMARY KEY",
            $"{DataTable.Cols.TimePeriodId} INTEGER NOT NULL",
            $"{DataTable.Cols.GeographicLevel} VARCHAR NOT NULL",
            .. dataSetVersion.LocationMetas.Select(location =>
                $"{DataTable.Cols.LocationId(location)} INTEGER NOT NULL"),
            .. dataSetVersion.FilterMetas.Select(filter => $"{DataTable.Cols.Filter(filter)} INTEGER NOT NULL"),
            .. dataSetVersion.IndicatorMetas.Select(indicator =>
                $"{DataTable.Cols.Indicator(indicator)} VARCHAR NOT NULL"),
        ];

        await duckDbConnection.SqlBuilder(
                $"""
                 CREATE TABLE {DataTable.TableName:raw}
                 ({columns.JoinToString(",\n"):raw})
                 """)
            .ExecuteAsync(cancellationToken: cancellationToken);

        string[] insertColumns =
        [
            "nextval('data_seq') AS id",
            $"{TimePeriodsTable.Ref().Id} AS {DataTable.Cols.TimePeriodId}",
            DataSourceTable.Ref.GeographicLevel,
            .. dataSetVersion.LocationMetas.Select(location =>
                $"COALESCE({LocationOptionsTable.Ref(location).Id}, 0) AS {DataTable.Cols.LocationId(location)}"),
            .. dataSetVersion.FilterMetas.Select(filter =>
                $"COALESCE({FilterOptionsTable.Ref(filter).Id}, 0) AS {DataTable.Cols.Filter(filter)}"),
            .. dataSetVersion.IndicatorMetas.Select(DataTable.Cols.Indicator),
        ];

        string[] insertJoins =
        [
            .. dataSetVersion.LocationMetas.Select(
                location =>
                {
                    var codeColumns = GetLocationCodeColumns(location.Level);

                    string[] conditions =
                    [
                        .. codeColumns.Select(col =>
                            $"{LocationOptionsTable.Ref(location).Col(col.Name)} = {DataSourceTable.Ref.Col(col.CsvName)}"),
                        $"{LocationOptionsTable.Ref(location).Label} = {DataSourceTable.Ref.Col(location.Level.CsvNameColumn())}"
                    ];

                    return $"""
                            LEFT JOIN {LocationOptionsTable.TableName} AS {LocationOptionsTable.Alias(location)}
                            ON {conditions.JoinToString(" AND ")}
                            """;
                }
            ),
            .. dataSetVersion.FilterMetas.Select(
                filter => $"""
                           LEFT JOIN {FilterOptionsTable.TableName} AS {FilterOptionsTable.Alias(filter)}
                           ON {FilterOptionsTable.Ref(filter).FilterColumn} = '{filter.Column}'
                           AND {FilterOptionsTable.Ref(filter).Label} = {DataSourceTable.Ref.Col(filter.Column)}
                           """
            ),
            $"""
             JOIN {TimePeriodsTable.TableName}
             ON {TimePeriodsTable.Ref().Period} = {DataSourceTable.Ref.TimePeriod}
             AND {TimePeriodsTable.Ref().Identifier} = {DataSourceTable.Ref.TimeIdentifier}
             """
        ];

        var dataFilePath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);

        await duckDbConnection.SqlBuilder(
            $"""
             INSERT INTO {DataTable.TableName:raw}
             SELECT
             {insertColumns.JoinToString(",\n"):raw}
             FROM read_csv('{dataFilePath:raw}', ALL_VARCHAR = true) AS {DataSourceTable.TableName:raw}
             {insertJoins.JoinToString('\n'):raw}
             ORDER BY
             {DataSourceTable.Ref.GeographicLevel:raw} ASC,
             {DataSourceTable.Ref.TimePeriod:raw} DESC
             """
        ).ExecuteAsync(cancellationToken: cancellationToken);
    }

    private static LocationColumn[] GetLocationCodeColumns(GeographicLevel geographicLevel)
    {
        return geographicLevel switch
        {
            GeographicLevel.LocalAuthority =>
            [
                new LocationColumn(Name: LocationOptionsTable.Cols.Code,
                    CsvName: GeographicLevelUtils.LocalAuthorityCsvColumns.NewCode),
                new LocationColumn(Name: LocationOptionsTable.Cols.OldCode,
                    CsvName: GeographicLevelUtils.LocalAuthorityCsvColumns.OldCode)
            ],
            GeographicLevel.Provider =>
            [
                new LocationColumn(Name: LocationOptionsTable.Cols.Ukprn,
                    CsvName: GeographicLevelUtils.ProviderCsvColumns.Ukprn)
            ],
            GeographicLevel.RscRegion => [],
            GeographicLevel.School =>
            [
                new LocationColumn(Name: LocationOptionsTable.Cols.Urn,
                    CsvName: GeographicLevelUtils.SchoolCsvColumns.Urn),
                new LocationColumn(Name: LocationOptionsTable.Cols.LaEstab,
                    CsvName: GeographicLevelUtils.SchoolCsvColumns.LaEstab)
            ],
            _ =>
            [
                new LocationColumn(Name: LocationOptionsTable.Cols.Code,
                    CsvName: geographicLevel.CsvCodeColumns().First())
            ],
        };
    }

    private record LocationColumn(string Name, string CsvName);

    private static class DataSourceTable
    {
        public const string TableName = "data_source";

        public static readonly TableRef Ref = new(TableName);

        public class TableRef(string table)
        {
            public readonly string TimePeriod = $"{table}.time_period";
            public readonly string TimeIdentifier = $"{table}.time_identifier";
            public readonly string GeographicLevel = $"{table}.geographic_level";

            public string Col(string column) => $"{table}.\"{column}\"";
        }
    }
}
