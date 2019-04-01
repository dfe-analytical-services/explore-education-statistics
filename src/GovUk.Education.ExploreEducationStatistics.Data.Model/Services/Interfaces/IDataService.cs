using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IDataService<TEntity> where TEntity : class
    {
        Task<int> Count();

        int Count(Expression<Func<TEntity, bool>> expression);

        IEnumerable<TEntity> All();

        TEntity Find(object id);
        
        TEntity Find(object id, List<Expression<Func<TEntity, object>>> include);
        
        IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
            List<Expression<Func<TEntity, object>>> include = null);

        int Max(Expression<Func<TEntity, int>> expression);
        
        long TopWithPredicate(Expression<Func<TEntity, long>> expression, Expression<Func<TEntity, bool>> predicate);
    }
}