using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public abstract class AbstractRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly StatisticsDbContext _context;

        protected AbstractRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        protected DbSet<TEntity> DbSet()
        {
            return _context.Set<TEntity>();
        }

        public Task<int> CountAsync()
        {
            return DbSet().CountAsync();
        }

        public int Count(Expression<Func<TEntity, bool>> expression)
        {
            return DbSet().Count(expression);
        }

        public IQueryable<TEntity> All(List<Expression<Func<TEntity, object>>> include = null)
        {
            var queryable = DbSet().AsQueryable();
            include?.ForEach(i => queryable = queryable.Include(i));
            return queryable;
        }

        public bool Exists(TKey id)
        {
            return Find(id) != null;
        }

        public TEntity Find(TKey id, List<Expression<Func<TEntity, object>>> include)
        {
            var queryable = DbSet().AsQueryable();
            include.ForEach(i => queryable = queryable.Include(i));
            return queryable
                .FindByPrimaryKey(_context, id)
                .SingleOrDefault();
        }

        public TEntity Find(TKey id)
        {
            return DbSet().Find(id);
        }

        public async Task<TEntity> FindAsync(TKey id)
        {
            return await DbSet().FindAsync(id);
        }

        public Either<ActionResult, TEntity> FindOrNotFound(TKey id)
        {
            return Find(id) ?? new Either<ActionResult, TEntity>(new NotFoundResult());
        }

        public async Task<Either<ActionResult, TEntity>> FindOrNotFoundAsync(TKey id)
        {
            return await FindAsync(id) ?? new Either<ActionResult, TEntity>(new NotFoundResult());
        }

        public IQueryable<TEntity> Find(TKey[] ids)
        {
            // TODO EES-711 - this code needs improving following upgrade to EF for Core 3
            var idField = typeof(TEntity).GetProperty("Id");

            var list = ids.ToList();
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var methodInfo = typeof(List<TKey>).GetMethod("Contains");
            var body = Expression.Call(Expression.Constant(list, typeof(List<TKey>)), methodInfo, Expression.MakeMemberAccess(parameter, idField));
            var predicateExpression = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            return DbSet().Where(predicateExpression);
        }

        public IQueryable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
            List<Expression<Func<TEntity, object>>> include = null)
        {
            var queryable = DbSet().Where(expression);
            include?.ForEach(i => queryable = queryable.Include(i));
            return queryable;
        }

        public int Max(Expression<Func<TEntity, int>> expression)
        {
            return DbSet().Max(expression);
        }

        public Guid TopWithPredicate(Expression<Func<TEntity, Guid>> expression,
            Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet().Where(predicate)
                .OrderByDescending(expression)
                .Select(expression)
                .FirstOrDefault();
        }

        public TEntity Remove(TKey id)
        {
            var entity = Find(id) ?? throw new ArgumentException("Entity not found", nameof(id));
            return DbSet().Remove(entity).Entity;
        }

        public async Task<TEntity> RemoveAsync(TKey id)
        {
            var entity = await FindAsync(id) ?? throw new ArgumentException("Entity not found", nameof(id));
            return DbSet().Remove(entity).Entity;
        }

        protected async Task<TEntity> UpdateAsync(TKey id)
        {
            var entity = await FindAsync(id) ?? throw new ArgumentException("Entity not found", nameof(id));
            return DbSet().Update(entity).Entity;
        }

        protected static SqlParameter CreateIdListType(string parameterName, IEnumerable<Guid> values)
        {
            return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListGuidType");
        }

        protected static SqlParameter CreateIdListType(string parameterName, IEnumerable<string> values)
        {
            return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListVarcharType");
        }

        protected static SqlParameter CreateListType(string parameterName, object value, string typeName)
        {
            return new SqlParameter(parameterName, value)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName
            };
        }
    }
}