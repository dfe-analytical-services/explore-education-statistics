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
#pragma warning disable EF1002
    public async Task<bool> KeyExistsAtJsonbPath<TDbContext, TRowId>(TDbContext context,
        JsonbPathRequest<TRowId> request,
        string keyValue,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", request.PathSegments);
        var keyValueParam = new NpgsqlParameter("keyValue", keyValue);
        var rowIdParam = new NpgsqlParameter("rowId", request.RowId);

        // We use the COALESCE function below to ensure that we're testing the existence of
        // the given key against a non-null JSON fragment. Without this, an exception is thrown
        // whereas we are wanting to return false. 
        var response = await context
            .Set<JsonBool>()
            .FromSqlRaw(
                sql: $"""
                      SELECT COALESCE("{request.JsonbColumnName}" #> @jsonPath, '[]'::jsonb)
                      ? @keyValue "{nameof(JsonBool.BoolValue)}"
                      FROM "{request.TableName}"
                      WHERE "{request.IdColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, keyValueParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return response?.BoolValue is not null
               && JsonSerializer.Deserialize<bool>(response.BoolValue);
    }
#pragma warning restore EF1002

#pragma warning disable EF1002
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
                      SELECT "{request.JsonbColumnName}" #> @jsonPath "{nameof(JsonFragment.JsonValue)}"
                      FROM "{request.TableName}"
                      WHERE "{request.IdColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return response?.JsonValue is not null
            ? JsonSerializer.Deserialize<TResponse>(response.JsonValue)
            : null;
    }
#pragma warning restore EF1002

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
            .OnSuccessDo(updateResult => SetJsonbAtPath(
                context: context,
                request: request,
                value: updateResult,
                cancellationToken: cancellationToken));
    }
}
