using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

/// <summary>
/// This helper allows us to bypass units of work that are incompatible with In-Memory databases, which do not
/// support transactions, exclusive locks etc.
/// </summary>
public class InMemoryDatabaseHelper : IDatabaseHelper
{
    public Task DoInTransaction(DbContext context, Func<Task> transactionalUnit)
    {
        return transactionalUnit.Invoke();
    }

    public Task<TResult> DoInTransaction<TResult>(DbContext context, Func<Task<TResult>> transactionalUnit)
    {
        return transactionalUnit.Invoke();
    }

    public Task DoInTransaction<TResult>(DbContext context, Action transactionalUnit)
    {
        return Task.FromResult(transactionalUnit);
    }

    public Task<TResult> DoInTransaction<TResult>(DbContext context, Func<TResult> transactionalUnit)
    {
        return Task.FromResult(transactionalUnit.Invoke());
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