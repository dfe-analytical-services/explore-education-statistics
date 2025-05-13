using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetLocationRepository(
    IDuckDbConnection duckDbConnection,
    IDataSetVersionPathResolver dataSetVersionPathResolver)
    : IParquetLocationRepository
{
    public async Task<IList<ParquetLocationOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListOptionsById<ParquetLocationOption>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: ["*"],
            cancellationToken: cancellationToken);
    }

    public async Task<IList<ParquetLocationOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<IDataSetQueryLocation> locations,
        CancellationToken cancellationToken = default)
    {
        var locationsByLevel = locations
            .GroupBy(location => location.ParsedLevel())
            .ToList();

        var allOptions = new List<ParquetLocationOption>();

        foreach (var levelGroup in locationsByLevel)
        {
            var columnValues = levelGroup
                .Select(MapLocationToColumnValue)
                .ToList();

            var whereBuilder = new DuckDbSqlBuilder();

            var groups = columnValues
                .GroupBy(cv => cv.Column)
                .ToList();

            foreach (var (group, index) in groups.WithIndex())
            {
                whereBuilder.AppendLine($"{group.Key:raw} IN ({group.Select(cv => cv.Value)})");

                if (!groups.IsLastIndex(index))
                {
                    whereBuilder.AppendLiteral("OR ");
                }
            }

            var command = duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM '{dataSetVersionPathResolver.LocationsPath(dataSetVersion):raw}'
                 WHERE {whereBuilder}
                 """
            );

            var optionMetas = await command
                .QueryAsync<ParquetLocationOption>(cancellationToken: cancellationToken);

            allOptions.AddRange(optionMetas);
        }

        return allOptions;
    }

    private static (string Column, string Value) MapLocationToColumnValue(IDataSetQueryLocation location)
    {
        return location switch
        {
            DataSetQueryLocationCode locationCode =>
                (Column: LocationOptionsTable.Cols.Code, Value: locationCode.Code),

            DataSetQueryLocationId locationId =>
                (Column: LocationOptionsTable.Cols.PublicId, Value: locationId.Id),

            DataSetQueryLocationLocalAuthorityCode localAuthority =>
                (Column: LocationOptionsTable.Cols.Code, Value: localAuthority.Code),
            DataSetQueryLocationLocalAuthorityOldCode localAuthority =>
                (Column: LocationOptionsTable.Cols.OldCode, Value: localAuthority.OldCode),

            DataSetQueryLocationProviderUkprn provider =>
                (Column: LocationOptionsTable.Cols.Ukprn, Value: provider.Ukprn),

            DataSetQueryLocationSchoolUrn school =>
                (Column: LocationOptionsTable.Cols.Urn, Value: school.Urn),
            DataSetQueryLocationSchoolLaEstab { LaEstab: not null } school =>
                (Column: LocationOptionsTable.Cols.LaEstab, Value: school.LaEstab),

            _ => throw new ArgumentOutOfRangeException(nameof(location))
        };
    }

    public async Task<IList<IdPublicIdPair>> ListOptionPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListOptionsById<IdPublicIdPair>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: [LocationOptionsTable.Cols.Id, LocationOptionsTable.Cols.PublicId],
            cancellationToken: cancellationToken);
    }

    private async Task<List<T>> ListOptionsById<T>(
        DataSetVersion dataSetVersion,
        IList<int> ids,
        IList<string> columns,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var columnsFragments = new DuckDbSqlBuilder();

        columnsFragments.AppendRange(
            columns.Select(col => (FormattableString)$"{col:raw}"),
            joinString: ",\n"
        );

        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT {columnsFragments}
             FROM '{dataSetVersionPathResolver.LocationsPath(dataSetVersion):raw}'
             WHERE {LocationOptionsTable.Cols.Id:raw} IN ({ids})
             """
        );

        return (await command.QueryAsync<T>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<ISet<GeographicLevel>> ListLevels(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT DISTINCT {LocationOptionsTable.Cols.Level:raw}
             FROM '{dataSetVersionPathResolver.LocationsPath(dataSetVersion):raw}'
             """);

        var levels = await command.QueryAsync<string>(cancellationToken: cancellationToken);

        return levels
            .Select(EnumUtil.GetFromEnumValue<GeographicLevel>)
            .ToHashSet();
    }

}
