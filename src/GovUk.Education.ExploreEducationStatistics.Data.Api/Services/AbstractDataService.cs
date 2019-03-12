using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
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

        private DbSet<TEntity> DbSet()
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

        public IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression)
        {
            return DbSet().Where(expression);
        }

        public int Max(Expression<Func<TEntity, int>> expression)
        {
            return DbSet().Max(expression);
        }

        public int TopWithPredicate(Expression<Func<TEntity, int>> expression, Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet().Where(predicate)
                .OrderByDescending(expression)
                .Select(expression)
                .FirstOrDefault();
        }
    }
}