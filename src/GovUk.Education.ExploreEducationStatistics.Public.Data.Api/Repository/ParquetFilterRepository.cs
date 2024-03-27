using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetFilterRepository(
    IDuckDbConnection duckDbConnection,
    IParquetPathResolver parquetPathResolver)
    : IParquetFilterRepository
{
    public async Task<IList<ParquetFilterOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListOptionsById<ParquetFilterOption>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: ["*"],
            cancellationToken: cancellationToken
        );
    }

    public async Task<IList<ParquetFilterOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<string> publicIds,
        CancellationToken cancellationToken = default)
    {
        var publicIdsList = publicIds.ToList();

        if (publicIdsList.Count == 0)
        {
            return new List<ParquetFilterOption>();
        }

        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT *
             FROM '{parquetPathResolver.FiltersPath(dataSetVersion):raw}'
             WHERE {FiltersTable.Cols.PublicId:raw} IN ({publicIdsList})
             """
        );

        return (await command.QueryAsync<ParquetFilterOption>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<IList<IdPublicIdPair>> ListOptionPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListOptionsById<IdPublicIdPair>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: [FiltersTable.Cols.Id, FiltersTable.Cols.PublicId],
            cancellationToken: cancellationToken
        );
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
             FROM '{parquetPathResolver.FiltersPath(dataSetVersion):raw}'
             WHERE {FiltersTable.Cols.Id:raw} IN ({ids})
             """
        );

        return (await command.QueryAsync<T>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<ISet<string>> ListFilterIds(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT DISTINCT {FiltersTable.Cols.ColumnName:raw}
             FROM '{parquetPathResolver.FiltersPath(dataSetVersion):raw}'
             """);

        var cols = await command
            .QueryAsync<string>(cancellationToken: cancellationToken);

        return cols.ToHashSet();
    }
}
