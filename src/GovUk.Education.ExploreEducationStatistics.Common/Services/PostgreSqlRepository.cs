using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PostgreSqlRepository : IPostgreSqlRepository
{
    public async Task<TResponse> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        string tableName,
        string idColumnName,
        string jsonColumnName,
        TRowId rowId,
        string[] jsonPathSegments,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", jsonPathSegments);
        var rowIdParam = new NpgsqlParameter("rowId", rowId);

        var response = await context
            .Set<JsonFragment>()
            .FromSqlRaw(
                sql: $"""
                      SELECT "{jsonColumnName}"#>@jsonPath "JsonString" FROM "{tableName}"
                      WHERE "{idColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return response is not null
            ? JsonSerializer.Deserialize<TResponse>(response.JsonString)
            : null;
    }

#pragma warning disable EF1002
    public async Task UpdateJsonbByPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        string tableName,
        string idColumnName,
        string jsonColumnName,
        TRowId rowId,
        string[] jsonPathSegments,
        TValue value,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", jsonPathSegments);
        var rowIdParam = new NpgsqlParameter("rowId", rowId);
        var valueParam = new NpgsqlParameter("value", NpgsqlDbType.Json)
        {
            Value = JsonSerializer.Serialize(value)
        };
        
        await context
            .Database
            .ExecuteSqlRawAsync(
                sql: $"""
                      UPDATE "{tableName}" SET "{jsonColumnName}" =
                      jsonb_set("{jsonColumnName}", @jsonPath, to_jsonb(@value))
                      WHERE "{idColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, valueParam, rowIdParam],
                cancellationToken: cancellationToken);
    }
#pragma warning restore EF1002
}
