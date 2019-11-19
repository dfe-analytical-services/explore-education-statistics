using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
{
    public class PersistenceHelper<TEntity, TEntityId> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _entitySet;
        private readonly ValidationErrorMessages _notFoundErrorMessage;
            
        public PersistenceHelper(
            DbContext context,
            DbSet<TEntity> entitySet,
            ValidationErrorMessages notFoundErrorMessage = ValidationErrorMessages.EntityNotFound)
        {
            _context = context;
            _entitySet = entitySet;
            _notFoundErrorMessage = notFoundErrorMessage;
        }
        
        public Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id, 
            Func<TEntity, Task<Either<ValidationResult, T>>> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null)   
        {
            return HandleValidationErrorsAsync(
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
                        ? ValidationResult(ValidationErrorMessages.EntityNotFound)
                        : new Either<ValidationResult, TEntity>(entity);
                },
                successAction.Invoke);
        } 
        
        public Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id, 
            Func<TEntity, T> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null)
        {
            Task<T> Success(TEntity entity) => Task.FromResult(successAction.Invoke(entity));

            return CheckEntityExists(
                id, 
                Success,
                hydrateEntityFn);
        } 
        
        public Task<Either<ValidationResult, T>> CheckEntityExists<T>(
            TEntityId id, 
            Func<TEntity, Task<T>> successAction,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> hydrateEntityFn = null)
        {
            async Task<Either<ValidationResult, T>> Success(TEntity entity)
            {
                var result = await successAction.Invoke(entity);
                return new Either<ValidationResult, T>(result);
            }

            return CheckEntityExists(
                id, 
                Success,
                hydrateEntityFn);
        } 
    }
}