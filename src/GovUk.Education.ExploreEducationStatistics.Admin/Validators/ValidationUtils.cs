#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationUtils
{
    public static BadRequestObjectResult ValidationActionResult(ValidationErrorMessages message)
    {
        return Common.Validators.ValidationUtils.ValidationResult(message);
    }

    public static BadRequestObjectResult ValidationActionResult(
        IEnumerable<ValidationErrorMessages> messages
    )
    {
        return Common.Validators.ValidationUtils.ValidationResult(messages);
    }

    public static Either<ActionResult, T> NotFound<T>()
    {
        return new NotFoundResult();
    }

    public static bool IsNotFound(ActionResult actionResult)
    {
        return actionResult is NotFoundResult;
    }

    public static bool HasValidationError(
        ActionResult actionResult,
        ValidationErrorMessages validationMessage
    )
    {
        var badRequest = actionResult as BadRequestObjectResult;
        var validationProblem = badRequest?.Value as ValidationProblemViewModel;
        var validationErrors = validationProblem?.Errors;
        return validationErrors != null
            && validationErrors.Any(error => error.Code == validationMessage.ToString());
    }
}
