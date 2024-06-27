using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPostgreSqlRepository
{
    Task<TResponse> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        string tableName,
        string idColumnName,
        string jsonColumnName,
        TRowId rowId,
        string[] jsonPathSegments,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
        where TResponse : class;
    
    Task UpdateJsonbByPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        string tableName,
        string idColumnName,
        string jsonColumnName,
        TRowId rowId,
        string[] jsonPathSegments,
        TValue value,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext;
}
