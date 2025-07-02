#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
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
    public async Task<bool> KeyExistsAtJsonbPath<TDbContext, TRowId>(
        TDbContext context,
        string schemaName,
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
        var result = await context.Database
            .SqlQueryRaw<bool>(
                sql: $"""
                      SELECT COALESCE("{request.JsonbColumnName}" #> @jsonPath, '[]'::jsonb)
                      ? @keyValue "Value"
                      FROM "{schemaName}"."{request.TableName}"
                      WHERE "{request.IdColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, keyValueParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return result;
    }
#pragma warning restore EF1002

#pragma warning disable EF1002
    public async Task<TResponse?> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        string schemaName,
        JsonbPathRequest<TRowId> request,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class
    {
        var jsonPathParam = new NpgsqlParameter("jsonPath", request.PathSegments);
        var rowIdParam = new NpgsqlParameter("rowId", request.RowId);

        var result = await context.Database
            .SqlQueryRaw<string>(
                sql: $"""
                      SELECT "{request.JsonbColumnName}" #> @jsonPath "Value"
                      FROM "{schemaName}"."{request.TableName}"
                      WHERE "{request.IdColumnName}" = @rowId
                      """,
                parameters: [jsonPathParam, rowIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return result is not null
            ? JsonSerializer.Deserialize<TResponse>(result)
            : null;
    }
#pragma warning restore EF1002

#pragma warning disable EF1002
    public async Task<TValue?> SetJsonbAtPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        string schemaName,
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
                      UPDATE "{schemaName}"."{request.TableName}" SET "{request.JsonbColumnName}" =
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
        string schemaName,
        JsonbPathRequest<TRowId> request,
        Func<TValue?, Task<Either<TFailure, TValue?>>> updateValueFn,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class
    {
        var json = await GetJsonbFromPath<TDbContext, TRowId, TValue>(
            context,
            schemaName,
            request,
            cancellationToken);

        return await updateValueFn
            .Invoke(json)
            .OnSuccessDo(updateResult => SetJsonbAtPath(
                context: context,
                schemaName: schemaName,
                request: request,
                value: updateResult,
                cancellationToken: cancellationToken));
    }
}
