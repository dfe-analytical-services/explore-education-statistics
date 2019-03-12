using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IDataService<TEntity> where TEntity : class
    {
        Task<int> Count();

        int Count(Expression<Func<TEntity, bool>> expression);

        IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression);

        int Max(Expression<Func<TEntity, int>> expression);
        
        int TopWithPredicate(Expression<Func<TEntity, int>> expression, Expression<Func<TEntity, bool>> predicate);
    }
}