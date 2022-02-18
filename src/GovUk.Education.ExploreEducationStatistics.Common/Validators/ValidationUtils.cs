#nullable enable
using System;
using System.Linq;
using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators
{
    public static class ValidationUtils
    {
        public static ActionResult ValidationResult(params Enum[] errors)
        {
            return ValidationResult(errors.Select(error => error.ToString()).ToArray());
        }
        
        public static ActionResult ValidationResult(params string[] errors)
        {
            var problemDetails = new ValidationProblemDetails(
                errors.ToDictionary(
                    _ => string.Empty,
                    error => new[] {error}))
            {
                Status = (int) HttpStatusCode.BadRequest,
                Detail = "Please refer to the errors property for additional details"
            };

            return new BadRequestObjectResult(problemDetails);
        }

        public static Either<ActionResult, T> ValidationResult<T>(Enum error)
        {
            return ValidationResult(error);
        }
    }
}
