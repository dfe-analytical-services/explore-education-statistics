using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods
{
    public static class ControllerExtensions
    {
        public static ActionResult<T> HandlingValidationErrors<T, R>(
            this ControllerBase controller,
            Func<Either<ValidationResult, R>> validationErrorsRaisingAction,
            Func<R, ActionResult> onSuccessAction)
        {
            var validationResults = validationErrorsRaisingAction.Invoke();

            if (validationResults.IsRight)
            {
                return onSuccessAction.Invoke(validationResults.Right);
            }
            
            ValidationUtils.AddErrors(controller.ModelState, validationResults.Left);
            return controller.ValidationProblem(new ValidationProblemDetails(controller.ModelState));
        }
        
        public static async Task<ActionResult<T>> HandlingValidationErrorsAsync<T>(
            this ControllerBase controller,
            Func<Task<Either<ValidationResult, T>>> validationErrorsRaisingAction,
            Func<T, ActionResult> onSuccessAction) 
        {
            var validationResults = await validationErrorsRaisingAction.Invoke();

            if (validationResults.IsRight)
            {
                return onSuccessAction.Invoke(validationResults.Right);
            }
            
            ValidationUtils.AddErrors(controller.ModelState, validationResults.Left);
            return controller.ValidationProblem(new ValidationProblemDetails(controller.ModelState));
        }
    }
}