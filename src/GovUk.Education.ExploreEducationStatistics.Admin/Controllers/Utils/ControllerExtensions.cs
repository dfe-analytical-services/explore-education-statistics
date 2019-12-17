using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
{
    public static class ControllerExtensions
    {
        // TODO EES-935 - replace with method chaining
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
        
        // TODO EES-935 - replace with method chaining
        public static async Task<ActionResult> HandlingValidationErrorsAsyncNoReturn<T>(
            this ControllerBase controller,
            Func<Task<Either<ValidationResult, T>>> validationErrorsRaisingAction,
            Func<ActionResult> onSuccessAction) 
        {
            var validationResults = await validationErrorsRaisingAction.Invoke();

            if (validationResults.IsRight)
            {
                return onSuccessAction.Invoke();
            }
            
            ValidationUtils.AddErrors(controller.ModelState, validationResults.Left);
            return controller.ValidationProblem(new ValidationProblemDetails(controller.ModelState));
        }
    }

    public static class EitherTaskExtensions
    {
        public static async Task<ActionResult> HandleFailures<Tr>(
            this Task<Either<ValidationResult, Tr>> validationErrorsRaisingAction,
            ControllerBase controller) where Tr : ActionResult
        {
            var result = await validationErrorsRaisingAction;
            
            if (result.IsRight)
            {
                return result.Right;
            }

            ValidationUtils.AddErrors(controller.ModelState, result.Left);
            return controller.ValidationProblem(new ValidationProblemDetails(controller.ModelState));
        }
        
        public static async Task<ActionResult> HandleFailures<Tr>(
            this Task<Either<ActionResult, Tr>> validationErrorsRaisingAction) where Tr : ActionResult
        {
            var result = await validationErrorsRaisingAction;
            
            return result.IsRight ? result.Right : result.Left;
        }
    }
}