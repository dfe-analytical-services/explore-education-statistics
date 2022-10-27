using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

/// <summary>
/// Helper component for providing transactional support to allow atomic units of work to be committed to the database.
/// </summary>
/// <remarks>
/// Note that DbContext.SaveChanges() / DbContext.SaveChangesAsync() is still necessary to call in order to persist
/// changes to the database.
/// </remarks>
public interface ITransactionHelper
{
    Task DoInTransaction(DbContext context, Func<Task> transactionalUnit);
    
    Task<TResult> DoInTransaction<TResult>(DbContext context, Func<Task<TResult>> transactionalUnit);    
    
    Task DoInTransaction<TResult>(DbContext context, Action transactionalUnit);
    
    Task<TResult> DoInTransaction<TResult>(DbContext context,Func<TResult> transactionalUnit);
}