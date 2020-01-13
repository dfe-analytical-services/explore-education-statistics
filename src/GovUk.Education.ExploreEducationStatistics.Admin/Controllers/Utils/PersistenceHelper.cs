using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
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
        
        public Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity, TEntityId>(
            TEntityId id, 
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null) 
            where TEntity : class
        {
            return HandleErrorsAsync(
                async () =>
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
                },
                entity => Task.FromResult(new Either<ActionResult, TEntity>(entity)));
        }        
        
        public Task<Either<ActionResult, TEntity>> CheckEntityExists<TEntity>(
            Guid id, 
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null) 
            where TEntity : class
        {
            return CheckEntityExists<TEntity, Guid>(id, hydrateEntityFn);
        }
    }
}