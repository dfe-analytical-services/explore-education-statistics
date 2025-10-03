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
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IParquetFilterRepository
{
    public async Task<IList<ParquetFilterOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    )
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
        CancellationToken cancellationToken = default
    )
    {
        var publicIdsList = publicIds.ToList();

        if (publicIdsList.Count == 0)
        {
            return new List<ParquetFilterOption>();
        }

        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT *
            FROM '{dataSetVersionPathResolver.FiltersPath(dataSetVersion):raw}'
            WHERE {FilterOptionsTable.Cols.PublicId:raw} IN ({publicIdsList})
            """
        );

        return (await command.QueryAsync<ParquetFilterOption>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<IList<IdPublicIdPair>> ListOptionPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    )
    {
        return await ListOptionsById<IdPublicIdPair>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns:
            [
                FilterOptionsTable.Cols.Id,
                FilterOptionsTable.Cols.PublicId,
                FilterOptionsTable.Cols.FilterId,
                FilterOptionsTable.Cols.FilterColumn,
            ],
            cancellationToken: cancellationToken
        );
    }

    private async Task<List<T>> ListOptionsById<T>(
        DataSetVersion dataSetVersion,
        IList<int> ids,
        IList<string> columns,
        CancellationToken cancellationToken = default
    )
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var columnsFragments = new DuckDbSqlBuilder();

        columnsFragments.AppendRange(columns.Select(col => (FormattableString)$"{col:raw}"), joinString: ",\n");

        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT {columnsFragments}
            FROM '{dataSetVersionPathResolver.FiltersPath(dataSetVersion):raw}'
            WHERE {FilterOptionsTable.Cols.Id:raw} IN ({ids})
            """
        );

        return (await command.QueryAsync<T>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<Dictionary<string, string>> GetFilterColumnsById(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT DISTINCT 
               {FilterOptionsTable.Cols.FilterId:raw},
               {FilterOptionsTable.Cols.FilterColumn:raw}
            FROM '{dataSetVersionPathResolver.FiltersPath(dataSetVersion):raw}'
            """
        );

        var cols = await command.QueryAsync<(string FilterId, string FilterColumn)>(
            cancellationToken: cancellationToken
        );

        return cols.ToDictionary(tuple => tuple.FilterId, tuple => tuple.FilterColumn);
    }
}
