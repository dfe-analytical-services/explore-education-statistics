#nullable enable
using System.Transactions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

/// <summary>
/// These extension methods provide a convenient way to ensure a transaction is
/// created for a given unit of work.
/// </summary>
public static class DbContextTransactionExtensions
{
    /// <summary>
    /// This method ensures that a transaction is available for the given unit of work and
    /// is shared between any DbContexts in use during the unit of work.
    /// 
    /// If an existing transaction already exists, the unit of work will use the
    /// existing transaction rather than this method creating a new one.
    ///
    /// If an uncaught exception is thrown during the unit of work, this will result in
    /// a rollback, unless caught and suppressed by any parent unit of work that has its
    /// own transaction open.
    ///
    /// Note that if you have a unit of work that requires interaction with multiple
    /// DbContexts and any use the EnableRetryOnFailure() strategy, you should use any
    /// one of the DbContexts that use this strategy to call "RequireTransaction" on,
    /// and thereafter any other DbContexts that also use the EnableRetryOnFailure()
    /// strategy will be functional. Not doing this will result in
    /// InvalidOperationExceptions when those DbContexts are used. 
    /// </summary>
    public static async Task<TResult> RequireTransaction<TDbContext, TResult>(
        this TDbContext context,
        Func<Task<TResult>> transactionalUnit)
        where TDbContext : DbContext
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                using var transactionScope = CreateTransactionScope();
                var result = await transactionalUnit.Invoke();
                transactionScope.Complete();
                return result;
            });
    }

    /// <summary>
    /// See the description for <see cref="RequireTransaction{TDbContext,TResult}"/>.
    /// The only difference with this method is that it accepts a Func that does not have
    /// a return value.
    /// </summary>
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

    /// <summary>
    /// This method ensures that a transaction is available for the given unit of work and
    /// is shared between any DbContexts in use during the unit of work.
    /// 
    /// If an existing transaction already exists, the unit of work will use the
    /// existing transaction rather than this method creating a new one.
    ///
    /// If the unit of work returns a Left Either, this will result in a rollback
    /// unless ignored by any parent unit of work that has its own transaction
    /// open. It is good practice therefore for any parent unit of work that has also
    /// opened its own transaction to propagate the Left Either back to the top of the
    /// call chain.
    ///
    /// Note that if you have a unit of work that requires interaction with multiple
    /// DbContexts and any use the EnableRetryOnFailure() strategy, you should use any
    /// one of the DbContexts that use this strategy to call "RequireTransaction" on,
    /// and thereafter any other DbContexts that also use the EnableRetryOnFailure()
    /// strategy will be functional. Not doing this will result in
    /// InvalidOperationExceptions when those DbContexts are used. 
    /// </summary>
    public static async Task<Either<TFailure, TResult>> RequireTransaction<TDbContext, TFailure, TResult>(
        this TDbContext context,
        Func<Task<Either<TFailure, TResult>>> transactionalUnit)
        where TDbContext : DbContext
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                using var transactionScope = CreateTransactionScope();
                return await transactionalUnit
                    .Invoke()
                    .OnSuccessDo(transactionScope.Complete);
            });
    }

    private static TransactionScope CreateTransactionScope(
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return new(
            scopeOption: transactionScopeOption,
            transactionOptions: new TransactionOptions {IsolationLevel = isolationLevel},
            asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);
    }
}
