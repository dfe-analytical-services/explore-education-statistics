using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql;
using InterpolatedSql.Dapper;
using StackExchange.Profiling;
using DataTable = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables.DataTable;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetDataRepository(
    IDuckDbConnection duckDbConnection,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IParquetDataRepository
{
    private const string DataIdsAlias = "data_ids";

    public async Task<long> CountRows(
        DataSetVersion dataSetVersion,
        IInterpolatedSql where,
        CancellationToken cancellationToken = default
    )
    {
        using var _ = MiniProfiler.Current.Step($"{nameof(ParquetDataRepository)}.{nameof(CountRows)}");

        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT count(*)
            FROM '{dataSetVersionPathResolver.DataPath(dataSetVersion):raw}'
            """
        );

        command.AppendIf(!where.IsEmpty(), $"\nWHERE {where}");

        return await command.QuerySingleAsync<long>(cancellationToken: cancellationToken);
    }

    public async Task<IList<IDictionary<string, object?>>> ListRows(
        DataSetVersion dataSetVersion,
        IEnumerable<string> columns,
        IInterpolatedSql where,
        IEnumerable<Sort>? sorts = null,
        int page = 1,
        int pageSize = 1000,
        CancellationToken cancellationToken = default
    )
    {
        using var _ = MiniProfiler.Current.Step($"{nameof(ParquetDataRepository)}.{nameof(ListRows)}");

        var dataPath = dataSetVersionPathResolver.DataPath(dataSetVersion);

        var whereFragment = new DuckDbSqlBuilder().AppendIf(!where.IsEmpty(), $"WHERE {where}");

        var orderings = (sorts ?? []).Select(s => $"{s.Field} {s.Direction.ToString().ToUpper()}").ToList();

        var orderByFragment = new DuckDbSqlBuilder()
            .AppendIf(orderings.Count != 0, $"ORDER BY")
            .AppendRange(orderings, joinString: ",\n");

        var pageOffset = (page - 1) * pageSize;

        // We essentially split this query into two sub-queries:
        //
        // 1. The main query which is offset paginated and gathers the row ids
        // 2. Another query to fetch the rows using the ids from the main query (i.e. a 'deferred' join)
        //
        // This 'deferred join' technique is more efficient than a single query and helps to reduce
        // the performance penalty of using offset pagination having to scan through many rows.
        var command = duckDbConnection.SqlBuilder(
            $"""
            WITH {DataIdsAlias:raw} AS (
               SELECT {DataTable.Ref().Id:raw}
               FROM '{dataPath:raw}' AS {DataTable.TableName:raw}
               {whereFragment}
               {orderByFragment}
               LIMIT {pageSize}
               OFFSET {pageOffset}
            )
            SELECT {columns.Select(DataTable.Ref().Col).JoinToString(",\n"):raw}
            FROM '{dataPath:raw}' AS {DataTable.TableName:raw}
            JOIN {DataIdsAlias:raw} ON {DataIdsAlias:raw}.id = {DataTable.Ref().Id:raw}
            {orderByFragment}
            """
        );

        return (await command.QueryAsync(cancellationToken: cancellationToken))
            .Cast<IDictionary<string, object?>>()
            .AsList();
    }

    public async Task<ISet<string>> ListColumns(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var command = duckDbConnection.SqlBuilder(
            $"DESCRIBE SELECT * FROM '{dataSetVersionPathResolver.DataPath(dataSetVersion):raw}' LIMIT 1"
        );

        var columns = await command.QueryAsync<ParquetColumn>(cancellationToken: cancellationToken);

        return columns.Select(col => col.ColumnName).ToHashSet();
    }
}
