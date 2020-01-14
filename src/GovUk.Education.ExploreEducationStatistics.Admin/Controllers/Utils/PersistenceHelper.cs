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
    public class PersistenceHelper<TEntity, TEntityId, TDbContext> : IPersistenceHelper<TEntity, TEntityId> 
        where TEntity : class 
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        private readonly DbSet<TEntity> _entitySet;
            
        public PersistenceHelper(
            TDbContext context,
            DbSet<TEntity> entitySet)
        {
            _context = context;
            _entitySet = entitySet;
        }
        
        public Task<Either<ActionResult, TEntity>> CheckEntityExists(
            TEntityId id, 
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null)
        {
            return HandleErrorsAsync(
                async () =>
                {
                    var queryableEntities = _entitySet
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
    }
}