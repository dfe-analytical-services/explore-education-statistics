#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IPostgreSqlRepository
{
    Task<TResponse?> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class;

    Task<TValue?> UpdateJsonbByPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        TValue? value,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class;

    Task<Either<TFailure, TValue?>> UpdateJsonbByPath<TDbContext, TValue, TRowId, TFailure>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        Func<TValue?, Either<TFailure, TValue?>> updateValueFn,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class;
}
