using System;
using System.Threading.Tasks;
using System.Transactions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DbContextTransactionExtensions
{
    public static async Task<TResult> RequireTransaction<TDbContext, TResult>(
        this TDbContext context,
        Func<Task<TResult>> transactionalUnit)
        where TDbContext : DbContext
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                using var transactionScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted},
                    TransactionScopeAsyncFlowOption.Enabled);

                var result = await transactionalUnit.Invoke();
                transactionScope.Complete();
                return result;
            });
    }

    public static async Task<Either<TFailure, TResult>> RequireTransaction<TDbContext, TFailure, TResult>(
        this TDbContext context,
        Func<Task<Either<TFailure, TResult>>> transactionalUnit)
        where TDbContext : DbContext
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                using var transactionScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted},
                    TransactionScopeAsyncFlowOption.Enabled);

                return await transactionalUnit
                    .Invoke()
                    .OnSuccessDo(transactionScope.Complete);
            });
    }
    
    public static async Task RequireTransaction<TDbContext>(
        this TDbContext context,
        Func<Task> transactionalUnit)
        where TDbContext : DbContext
    {
        await RequireTransaction(context, async () =>
        {
            await transactionalUnit.Invoke();
            return Unit.Instance;
        });
    }

    public static Task RequireTransaction<TDbContext>(
        this TDbContext context, 
        Action transactionalUnit)
        where TDbContext : DbContext
    {
        return RequireTransaction(context, () =>
        {
            transactionalUnit.Invoke();
            return Task.CompletedTask;
        });
    }

    public static Task<TResult> RequireTransaction<TDbContext, TResult>(
        this TDbContext context, 
        Func<TResult> transactionalUnit)
        where TDbContext : DbContext
    {
        return RequireTransaction(context, () => Task.FromResult(transactionalUnit.Invoke()));
    }
}
