#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PostgreSqlRepository : IPostgreSqlRepository
{
    public async Task<TResponse?> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", request.PathSegments);
        var rowIdParam = new NpgsqlParameter("rowId", request.RowId);

        var response = await context
            .Set<JsonFragment>()
            .FromSqlRaw(
                sql: $"""
                    SELECT "{request.JsonbColumnName}"#>@jsonPath "{nameof(JsonFragment.JsonString)}" FROM "{request.TableName}"
                    WHERE "{request.IdColumnName}" = @rowId
                    """,
                parameters: [jsonPathParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return response?.JsonString is not null
            ? JsonSerializer.Deserialize<TResponse>(response.JsonString)
            : null;
    }

#pragma warning disable EF1002
    public async Task<TValue?> SetJsonbAtPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        TValue? value,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", request.PathSegments);
        var rowIdParam = new NpgsqlParameter("rowId", request.RowId);
        var valueParam = new NpgsqlParameter("value", NpgsqlDbType.Json)
        {
            Value = value is not null
                ? JsonSerializer.Serialize(value)
                : null
        };

        await context
            .Database
            .ExecuteSqlRawAsync(
                sql: $"""
                      UPDATE "{request.TableName}" SET "{request.JsonbColumnName}" =
                      jsonb_set("{request.JsonbColumnName}", @jsonPath, to_jsonb(@value))
                      WHERE "{request.IdColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, valueParam, rowIdParam],
                cancellationToken: cancellationToken);

        return value;
    }

#pragma warning restore EF1002

    public async Task<Either<TFailure, TValue?>> UpdateJsonbAtPath<TDbContext, TValue, TRowId, TFailure>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        Func<TValue?, Task<Either<TFailure, TValue?>>> updateValueFn,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class
    {
        var json = await GetJsonbFromPath<TDbContext, TRowId, TValue>(
            context,
            request,
            cancellationToken);

        return await updateValueFn
            .Invoke(json)
            .OnSuccessDo(updateResult => SetJsonbAtPath(context, request, updateResult, cancellationToken));
    }
}
