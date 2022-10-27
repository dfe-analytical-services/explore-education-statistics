using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface ITransactionHelper
{
    Task DoInTransaction(DbContext context, Func<Task> transactionalUnit);
    
    Task<TResult> DoInTransaction<TResult>(DbContext context, Func<Task<TResult>> transactionalUnit);    
    
    Task DoInTransaction<TResult>(DbContext context, Action transactionalUnit);
    
    Task<TResult> DoInTransaction<TResult>(DbContext context,Func<TResult> transactionalUnit);
}