#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public static class ValidationUtils
    {
        // TODO EES-919 - return ActionResults rather than ValidationResults
        public static ValidationResult ValidationResult(ValidationErrorMessages message)
        {
            return new ValidationResult(message.ToString());
        }

        // TODO EES-919 - return ActionResults rather than ValidationResults - as this work is done,
        // rename this to "ValidationResult"
        public static ActionResult ValidationActionResult(ValidationErrorMessages message)
        {
            return Common.Validators.ValidationUtils.ValidationResult(message);
        }

        public static ActionResult ValidationActionResult(IEnumerable<ValidationErrorMessages> messages)
        {
            ModelStateDictionary errors = new ModelStateDictionary();

            foreach (var message in messages)
            {
                errors.AddModelError(string.Empty, message.ToString());
            }

            return new BadRequestObjectResult(new ValidationProblemDetails(errors));
        }

        public static Either<ActionResult, T> NotFound<T>()
        {
            return new NotFoundResult();
        }
    }
}
