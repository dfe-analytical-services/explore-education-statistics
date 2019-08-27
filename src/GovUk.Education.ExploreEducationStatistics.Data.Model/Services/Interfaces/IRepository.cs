using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IRepository<TEntity, in TKey> where TEntity : class
    {
        Task<int> Count();

        int Count(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> All(List<Expression<Func<TEntity, object>>> include = null);

        TEntity Find(TKey id);

        IQueryable<TEntity> Find(TKey[] ids);

        TEntity Find(TKey id, List<Expression<Func<TEntity, object>>> include);
        
        IQueryable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
            List<Expression<Func<TEntity, object>>> include = null);

        int Max(Expression<Func<TEntity, int>> expression);
        
        long TopWithPredicate(Expression<Func<TEntity, long>> expression, Expression<Func<TEntity, bool>> predicate);
    }
}