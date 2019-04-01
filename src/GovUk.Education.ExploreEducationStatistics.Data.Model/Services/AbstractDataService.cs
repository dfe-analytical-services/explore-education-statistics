using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public abstract class AbstractDataService<TEntity> : IDataService<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger _logger;

        protected AbstractDataService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        protected DbSet<TEntity> DbSet()
        {
            return _context.Set<TEntity>();
        }

        public Task<int> Count()
        {
            return DbSet().CountAsync();
        }

        public int Count(Expression<Func<TEntity, bool>> expression)
        {
            return DbSet().Count(expression);
        }

        public IEnumerable<TEntity> All()
        {
            return DbSet();
        }

        public TEntity Find(object id, List<Expression<Func<TEntity, object>>> include)
        {
            var queryable = DbSet().AsQueryable();
            include.ForEach(i => queryable = queryable.Include(i));
            return queryable
                .FilterByPrimaryKey(_context, id)
                .SingleOrDefault();
        }
        
        public TEntity Find(object id)
        {
            return DbSet().Find(id);
        }

        public IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression,
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

        public long TopWithPredicate(Expression<Func<TEntity, long>> expression,
            Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet().Where(predicate)
                .OrderByDescending(expression)
                .Select(expression)
                .FirstOrDefault();
        }
    }
}