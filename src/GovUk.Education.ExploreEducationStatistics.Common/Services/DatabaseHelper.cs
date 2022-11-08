#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class DatabaseHelper : IDatabaseHelper
{
    public async Task DoInTransaction<TDbContext>(
        TDbContext context,
        Func<TDbContext> createDbContextDelegateFn,
        Func<TDbContext, Task> transactionalUnit)
        where TDbContext : DbContext
    {
        await DoInTransaction(context, createDbContextDelegateFn, async ctxDelegate =>
        {
            await transactionalUnit.Invoke(ctxDelegate);
            return Unit.Instance;
        });
    }

    public Task<TResult> DoInTransaction<TDbContext, TResult>(
        TDbContext context,
        Func<TDbContext> createDbContextDelegateFn,
        Func<TDbContext, Task<TResult>> transactionalUnit)
        where TDbContext : DbContext
    {
        var strategy = context.Database.CreateExecutionStrategy();
        
        return strategy.Execute(async () =>
        {
            // We use a delegate context here to allow us to retry on failure successfully when defining our own
            // transaction boundaries manually. See
            // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency#execution-strategies-and-transactions
            var ctxDelegate = createDbContextDelegateFn.Invoke();
            await using var transaction = await ctxDelegate.Database.BeginTransactionAsync();

            var result = await transactionalUnit.Invoke(ctxDelegate);
            
            await transaction.CommitAsync();
            await ctxDelegate.Database.CloseConnectionAsync();

            return result;
        });
    }

    public Task DoInTransaction<TDbContext>(
        TDbContext context, 
        Func<TDbContext> createDbContextDelegateFn,
        Action<TDbContext> transactionalUnit)
        where TDbContext : DbContext
    {
        return DoInTransaction(context, createDbContextDelegateFn, ctxDelegate =>
        {
            transactionalUnit.Invoke(ctxDelegate);
            return Task.CompletedTask;
        });
    }

    public Task<TResult> DoInTransaction<TDbContext, TResult>(
        TDbContext context, 
        Func<TDbContext> createDbContextDelegateFn,
        Func<TDbContext, TResult> transactionalUnit)
        where TDbContext : DbContext
    {
        return DoInTransaction(context, createDbContextDelegateFn, ctx => Task.FromResult(transactionalUnit.Invoke(ctx)));
    }
    
    public Task ExecuteWithExclusiveLock<TDbContext>(
        TDbContext dbContext,
        Func<TDbContext> createDbContextDelegateFn,
        string lockName,
        Func<TDbContext, Task> action)
        where TDbContext : DbContext
    {
        return DoInTransaction(dbContext, createDbContextDelegateFn, async ctx =>
        {
            await ctx.Database.ExecuteSqlRawAsync($"exec sp_getapplock '{lockName}', 'exclusive'");
            await action.Invoke(ctx);
        });
    }
}