using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

/// <summary>
/// This helper allows us to bypass units of work that are incompatible with In-Memory databases, which do not
/// support transactions, exclusive locks etc.
/// </summary>
public class InMemoryDatabaseHelper : IDatabaseHelper
{
    public Task DoInTransaction<TDbContext>(TDbContext context, Func<TDbContext, Task> transactionalUnit) where TDbContext : DbContext
    {
        return transactionalUnit.Invoke(context);
    }

    public async Task<TResult> DoInTransaction<TDbContext, TResult>(TDbContext context, Func<TDbContext, Task<TResult>> transactionalUnit) where TDbContext : DbContext
    {
        return await transactionalUnit.Invoke(context);
    }

    public Task DoInTransaction<TDbContext>(TDbContext context, Action<TDbContext> transactionalUnit) where TDbContext : DbContext
    {
        return Task.FromResult(transactionalUnit);
    }

    public Task<TResult> DoInTransaction<TDbContext, TResult>(TDbContext context, Func<TDbContext, TResult> transactionalUnit) where TDbContext : DbContext
    {
        return Task.FromResult(transactionalUnit.Invoke(context));
    }

    public Task ExecuteWithExclusiveLock<TDbContext>(
        TDbContext dbContext, 
        string lockName, 
        Func<TDbContext, Task> action) 
        where TDbContext : DbContext
    {
        return action.Invoke(dbContext);
    }
}