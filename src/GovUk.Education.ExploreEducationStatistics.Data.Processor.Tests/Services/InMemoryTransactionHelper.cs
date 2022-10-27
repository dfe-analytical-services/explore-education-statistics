using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

/// <summary>
/// This helper allows us to bypass transactional units of work when testing with In-Memory databases, which do not
/// support transactions.
/// </summary>
public class InMemoryTransactionHelper : ITransactionHelper
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
}