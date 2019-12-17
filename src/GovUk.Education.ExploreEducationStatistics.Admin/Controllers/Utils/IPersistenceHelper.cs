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
        // TODO EES-934 - replace with method chaining
        Task<Either<ActionResult, T>> CheckEntityExistsActionResult<T>(
            TEntityId id,
            Func<TEntity, Task<Either<ActionResult, T>>> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);

        // TODO EES-919 - return ActionResults rather than ValidationResults
        // TODO EES-934 - replace with method chaining
        Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id,
            Func<TEntity, Task<Either<ValidationResult, T>>> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);

        // TODO EES-919 - return ActionResults rather than ValidationResults
        // TODO EES-934 - replace with method chaining
        Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id,
            Func<TEntity, T> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);

        // TODO EES-919 - return ActionResults rather than ValidationResults
        // TODO EES-934 - replace with method chaining
        Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id,
            Func<TEntity, Task<T>> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);
        
        // TODO EES-919 - return ActionResults rather than ValidationResults
        Task<Either<ValidationResult, TEntity>> CheckEntityExistsChainable(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);
        
        Task<Either<ActionResult, TEntity>> CheckEntityExistsChainableActionResult(
            TEntityId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null);
    }
}