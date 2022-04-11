using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        protected TEntity Find(TKey id)
        {
            return DbSet().Find(id);
        }

        private async Task<TEntity> FindAsync(TKey id)
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

        public IQueryable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
            List<Expression<Func<TEntity, object>>> include = null)
        {
            var queryable = DbSet().Where(expression);
            include?.ForEach(i => queryable = queryable.Include(i));
            return queryable;
        }

        protected async Task<TEntity> RemoveAsync(TKey id)
        {
            var entity = await FindAsync(id) ?? throw new ArgumentException("Entity not found", nameof(id));
            return DbSet().Remove(entity).Entity;
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