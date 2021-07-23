using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IRepository<TEntity, in TKey> where TEntity : class
    {
        Task<int> CountAsync();

        int Count(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> All(List<Expression<Func<TEntity, object>>> include = null);

        bool Exists(TKey id);
        
        TEntity Find(TKey id);

        Task<TEntity> FindAsync(TKey id);
        
        Either<ActionResult, TEntity> FindOrNotFound(TKey id);

        Task<Either<ActionResult, TEntity>> FindOrNotFoundAsync(TKey id);

        IQueryable<TEntity> Find(TKey[] ids);

        TEntity Find(TKey id, List<Expression<Func<TEntity, object>>> include);
        
        IQueryable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
            List<Expression<Func<TEntity, object>>> include = null);

        int Max(Expression<Func<TEntity, int>> expression);
        
        Guid TopWithPredicate(Expression<Func<TEntity, Guid>> expression, Expression<Func<TEntity, bool>> predicate);

        TEntity Remove(TKey id);

        Task<TEntity> RemoveAsync(TKey id);
    }
}