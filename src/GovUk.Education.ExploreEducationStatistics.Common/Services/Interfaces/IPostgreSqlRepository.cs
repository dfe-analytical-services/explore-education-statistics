using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IPostgreSqlRepository
{
    Task<TResponse> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class;
    
    Task UpdateJsonbByPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        TValue value,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class;
    
    Task<TValue> UpdateJsonbByPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        Action<TValue> updateValueAction,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TValue : class;
}
