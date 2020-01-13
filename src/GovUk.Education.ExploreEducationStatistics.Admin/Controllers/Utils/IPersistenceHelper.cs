using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
{
    public interface IPersistenceHelper<TEntity, TEntityId> where TEntity : class
    {
        // TODO EES-919 - return ActionResults rather than ValidationResults
        // Rename this to "CheckEntityExists" when other methods below are removed or altered to return
        // ActionResults
        Task<Either<ActionResult, TEntity>> CheckEntityExistsActionResult(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);
    }
}