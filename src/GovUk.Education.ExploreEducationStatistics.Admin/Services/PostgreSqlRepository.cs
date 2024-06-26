using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PostgreSqlRepository : IPostgreSqlRepository
{
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
        var valueParam = new NpgsqlParameter("value", value);
        var rowIdParam = new NpgsqlParameter("rowId", rowId);

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
