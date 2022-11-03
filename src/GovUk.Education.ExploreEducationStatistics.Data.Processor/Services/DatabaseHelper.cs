using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DatabaseHelper : IDatabaseHelper
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
    
    public Task ExecuteWithExclusiveLock<TDbContext>(
        TDbContext dbContext,
        string lockName,
        Func<TDbContext, Task> action)
        where TDbContext : DbContext
    {
        return dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.ExecuteSqlRawAsync($"exec sp_getapplock '{lockName}', 'exclusive'");

            try
            {
                await action(dbContext);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}