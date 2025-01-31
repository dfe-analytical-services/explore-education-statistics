#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public class PersistenceHelper<TDbContext> : IPersistenceHelper<TDbContext>
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        public PersistenceHelper(
            TDbContext context)
        {
            _context = context;
        }

        public async Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity, TEntityId>(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null)
            where TEntity : class
        {
            var queryableEntities = _context.Set<TEntity>()
                .FindByPrimaryKey(_context, id);

            var hydratedEntities = hydrateEntityFn != null
                ? hydrateEntityFn.Invoke(queryableEntities)
                : queryableEntities;

            var entity = await hydratedEntities
                .FirstOrDefaultAsync();

            return entity == null
                ? new NotFoundResult()
                : new Either<ActionResult, TEntity>(entity);
        }

        public Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null)
            where TEntity : class
        {
            return CheckEntityExists<TEntity, Guid>(id, hydrateEntityFn);
        }

        public async Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
            where TEntity : class
        {
            var queryableEntities = _context.Set<TEntity>();

            var entity = await query.Invoke(queryableEntities)
                .FirstOrDefaultAsync();

            return entity == null
                ? new NotFoundResult()
                : new Either<ActionResult, TEntity>(entity);
        }

        public async Task<Either<ActionResult, TEntity?>> CheckOptionalEntityExists<TEntity>(Guid? id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null) where TEntity : class
        {
            return id.HasValue
                ? await CheckEntityExists(id.Value, hydrateEntityFn)
                : (TEntity) null;
        }
    }
}
