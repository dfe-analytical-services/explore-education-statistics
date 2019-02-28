using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public abstract class AbstractDataService<TEntity> : IDataService<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;

        protected AbstractDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        private DbSet<TEntity> DbSet()
        {
            return _context.Set<TEntity>();
        }

        public int Count()
        {
            return DbSet().Count();
        }
        
        public int Count(Expression<Func<TEntity, bool>> expression)
        {
            return DbSet().Count(expression);
        }

        public IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression)
        {
            return DbSet().Where(expression);
        }
    }
}