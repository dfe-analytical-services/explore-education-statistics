#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationUtils
{
    public static BadRequestObjectResult ValidationActionResult(ValidationErrorMessages message)
    {
        return Common.Validators.ValidationUtils.ValidationResult(message);
    }

    public static BadRequestObjectResult ValidationActionResult(IEnumerable<ValidationErrorMessages> messages)
    {
        return Common.Validators.ValidationUtils.ValidationResult(messages);
    }

    public static Either<ActionResult, T> NotFound<T>()
    {
        return new NotFoundResult();
    }
}
