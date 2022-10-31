using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IDatabaseHelper
{
    /// <summary>
    /// Helper method for providing transactional support to allow atomic units of work to be committed to the database.
    /// </summary>
    /// <remarks>
    /// Note that DbContext.SaveChanges() / DbContext.SaveChangesAsync() is still necessary to call in order to persist
    /// changes to the database.
    /// </remarks>
    Task DoInTransaction(DbContext context, Func<Task> transactionalUnit);
    
    /// <summary>
    /// Helper method for providing transactional support to allow atomic units of work to be committed to the database.
    /// </summary>
    /// <remarks>
    /// Note that DbContext.SaveChanges() / DbContext.SaveChangesAsync() is still necessary to call in order to persist
    /// changes to the database.
    /// </remarks>
    Task<TResult> DoInTransaction<TResult>(DbContext context, Func<Task<TResult>> transactionalUnit);    
    
    /// <summary>
    /// Helper method for providing transactional support to allow atomic units of work to be committed to the database.
    /// </summary>
    /// <remarks>
    /// Note that DbContext.SaveChanges() / DbContext.SaveChangesAsync() is still necessary to call in order to persist
    /// changes to the database.
    /// </remarks>
    Task DoInTransaction<TResult>(DbContext context, Action transactionalUnit);
    
    /// <summary>
    /// Helper method for providing transactional support to allow atomic units of work to be committed to the database.
    /// </summary>
    /// <remarks>
    /// Note that DbContext.SaveChanges() / DbContext.SaveChangesAsync() is still necessary to call in order to persist
    /// changes to the database.
    /// </remarks>
    Task<TResult> DoInTransaction<TResult>(DbContext context,Func<TResult> transactionalUnit);

    /// <summary>
    /// Obtains an exclusive lock within a new transaction and stops other transactions needing to acquire the
    /// same lock to run until they can obtain the lock.  The lock is released upon a transaction being committed
    /// or rolled back.
    /// </summary>
    Task ExecuteWithExclusiveLock<TDbContext>(
        TDbContext dbContext,
        string lockName,
        Func<TDbContext, Task> action)
        where TDbContext : DbContext;
}