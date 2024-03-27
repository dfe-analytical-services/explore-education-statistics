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

public class ParquetFilterOptionRepository(
    IDuckDbConnection duckDbConnection,
    IParquetPathResolver parquetPathResolver)
    : IParquetFilterOptionRepository
{
    public async Task<IEnumerable<ParquetFilterOption>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListById<ParquetFilterOption>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: ["*"],
            cancellationToken: cancellationToken
        );
    }

    public async Task<IEnumerable<ParquetFilterOption>> List(
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

        return await command.QueryAsync<ParquetFilterOption>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<IdPublicIdPair>> ListPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ListById<IdPublicIdPair>(
            dataSetVersion: dataSetVersion,
            ids: ids.ToList(),
            columns: [FiltersTable.Cols.Id, FiltersTable.Cols.PublicId],
            cancellationToken: cancellationToken
        );
    }

    private async Task<IEnumerable<T>> ListById<T>(
        DataSetVersion dataSetVersion,
        IList<int> ids,
        IList<string> columns,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return new List<T>();
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

        return await command.QueryAsync<T>(cancellationToken: cancellationToken);
    }
}
