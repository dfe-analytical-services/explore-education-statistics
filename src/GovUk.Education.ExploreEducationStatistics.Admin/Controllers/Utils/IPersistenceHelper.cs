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
        Task<Either<ActionResult, TEntity>> CheckEntityExists(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);
    }
}