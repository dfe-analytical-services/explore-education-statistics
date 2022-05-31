#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public interface IPersistenceHelper<TDbContext> where TDbContext : DbContext
    {
        Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity, TEntityId>(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null)
            where TEntity : class;

        Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null)
            where TEntity : class;

        Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
            where TEntity : class;

        Task<Either<ActionResult, TEntity?>> CheckOptionalEntityExists<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? hydrateEntityFn = null)
            where TEntity : class;
    }
}
