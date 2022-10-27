using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class TransactionHelper : ITransactionHelper
{
    public async Task DoInTransaction(DbContext context, Func<Task> transactionalUnit)
    {
        await DoInTransaction(context, async () =>
        {
            await transactionalUnit.Invoke();
            return Unit.Instance;
        });
    }

    public Task<TResult> DoInTransaction<TResult>(DbContext context, Func<Task<TResult>> transactionalUnit)
    {
        return context.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            var result = await transactionalUnit.Invoke();
            
            await transaction.CommitAsync();
            await context.Database.CloseConnectionAsync();

            return result;
        });
    }

    public Task DoInTransaction<TResult>(DbContext context, Action transactionalUnit)
    {
        return DoInTransaction(context, () => Task.FromResult(transactionalUnit));
    }

    public Task<TResult> DoInTransaction<TResult>(DbContext context, Func<TResult> transactionalUnit)
    {
        return DoInTransaction(context, () => Task.FromResult(transactionalUnit.Invoke()));
    }
}