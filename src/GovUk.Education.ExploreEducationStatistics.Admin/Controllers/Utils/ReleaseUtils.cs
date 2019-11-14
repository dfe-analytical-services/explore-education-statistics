using System;
using System.ComponentModel.DataAnnotations;
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
            ApplicationDbContext context,
            Guid releaseId, 
            Func<Release, Task<T>> successAction)
        {
            return HandleValidationErrorsAsync(
                async () =>
                {
                    var release = await context
                        .Releases
                        .FindByPrimaryKey(context, releaseId)
                        .FirstOrDefaultAsync();

                    return release == null
                        ? ValidationResult(ValidationErrorMessages.ReleaseNotFound)
                        : new Either<ValidationResult, Release>(release);
                },
                successAction.Invoke
            );
        } 
        
        public static Task<Either<ValidationResult, T>> CheckReleaseExists<T>(
            ApplicationDbContext context,
            Guid releaseId, 
            Func<Release, Task<Either<ValidationResult, T>>> successAction)
        {
            return HandleValidationErrorsAsync(
                async () =>
                {
                    var release = await context
                        .Releases
                        .FindByPrimaryKey(context, releaseId)
                        .FirstOrDefaultAsync();

                    return release == null
                        ? ValidationResult(ValidationErrorMessages.ReleaseNotFound)
                        : new Either<ValidationResult, Release>(release);
                },
                successAction.Invoke);
        } 
        
        public static Task<Either<ValidationResult, T>> CheckReleaseExists<T>(
            ApplicationDbContext context,
            Guid releaseId, 
            Func<Release, T> successAction)
        {
            return CheckReleaseExists(
                context,
                releaseId, 
                release => Task.FromResult(successAction.Invoke(release)));
        } 
        
    }
}