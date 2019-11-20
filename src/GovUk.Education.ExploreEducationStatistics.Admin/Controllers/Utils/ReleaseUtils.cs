using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
{
    public static class ReleaseUtils
    {
        public static Task<Either<ValidationResult, T>> CheckReleaseExists<T>(
            ContentDbContext context,
            Guid releaseId, 
            Func<Release, Task<Either<ValidationResult, T>>> successAction,
            Func<IQueryable<Release>, IQueryable<Release>> hydrateReleaseFn = null)
        {
            return HandleValidationErrorsAsync(
                async () =>
                {
                    var queryableReleases = context
                        .Releases
                        .FindByPrimaryKey(context, releaseId);

                    var hydratedReleases = hydrateReleaseFn != null
                        ? hydrateReleaseFn.Invoke(queryableReleases)
                        : queryableReleases;
                    
                    var release = await hydratedReleases
                        .FirstOrDefaultAsync();

                    return release == null
                        ? ValidationResult(ValidationErrorMessages.ReleaseNotFound)
                        : new Either<ValidationResult, Release>(release);
                },
                successAction.Invoke);
        } 
        
        public static Task<Either<ValidationResult, T>> CheckReleaseExists<T>(
            ContentDbContext context,
            Guid releaseId, 
            Func<Release, T> successAction,
            Func<IQueryable<Release>, IQueryable<Release>> hydrateReleaseFn = null)
        {
            Task<T> Success(Release release) => Task.FromResult(successAction.Invoke(release));

            return CheckReleaseExists(
                context,
                releaseId, 
                Success,
                hydrateReleaseFn);
        } 
        
        public static Task<Either<ValidationResult, T>> CheckReleaseExists<T>(
            ContentDbContext context,
            Guid releaseId, 
            Func<Release, Task<T>> successAction,
            Func<IQueryable<Release>, IQueryable<Release>> hydrateReleaseFn = null)
        {
            async Task<Either<ValidationResult, T>> Success(Release release)
            {
                var result = await successAction.Invoke(release);
                return new Either<ValidationResult, T>(result);
            }

            return CheckReleaseExists(
                context,
                releaseId, 
                Success,
                hydrateReleaseFn);
        } 
    }
}