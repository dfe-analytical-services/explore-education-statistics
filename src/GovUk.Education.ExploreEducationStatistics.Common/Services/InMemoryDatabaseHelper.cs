using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

/// <summary>
/// This helper allows us to bypass units of work that are incompatible with In-Memory databases, which do not
/// support transactions, exclusive locks etc.
/// </summary>
public class InMemoryDatabaseHelper(IDbContextSupplier dbContextSupplier) : IDatabaseHelper
{
    public Task DoInTransaction<TDbContext>(TDbContext context, Func<TDbContext, Task> transactionalUnit)
        where TDbContext : DbContext
    {
        var ctxDelegate = dbContextSupplier.CreateDbContextDelegate<TDbContext>();
        return transactionalUnit.Invoke(ctxDelegate);
    }

    public async Task<TResult> DoInTransaction<TDbContext, TResult>(
        TDbContext context,
        Func<TDbContext, Task<TResult>> transactionalUnit
    )
        where TDbContext : DbContext
    {
        var ctxDelegate = dbContextSupplier.CreateDbContextDelegate<TDbContext>();
        return await transactionalUnit.Invoke(ctxDelegate);
    }

    public Task DoInTransaction<TDbContext>(TDbContext context, Action<TDbContext> transactionalUnit)
        where TDbContext : DbContext
    {
        var ctxDelegate = dbContextSupplier.CreateDbContextDelegate<TDbContext>();
        transactionalUnit.Invoke(ctxDelegate);
        return Task.CompletedTask;
    }

    public Task<TResult> DoInTransaction<TDbContext, TResult>(
        TDbContext context,
        Func<TDbContext, TResult> transactionalUnit
    )
        where TDbContext : DbContext
    {
        var ctxDelegate = dbContextSupplier.CreateDbContextDelegate<TDbContext>();
        return Task.FromResult(transactionalUnit.Invoke(ctxDelegate));
    }

    public Task ExecuteWithExclusiveLock<TDbContext>(
        TDbContext dbContext,
        string lockName,
        Func<TDbContext, Task> action
    )
        where TDbContext : DbContext
    {
        var ctxDelegate = dbContextSupplier.CreateDbContextDelegate<TDbContext>();
        return action.Invoke(ctxDelegate);
    }
}
