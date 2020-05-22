using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class ValidationTestUtil
    {
        public static ValidationProblemDetails AssertValidationProblem(ActionResult result,
            ValidationErrorMessages message)
        {
            var validationProblem = AssertValidationProblemIsBadDetails(result);
            return AssertValidationProblemDetails(validationProblem, message);
        }

        public static ValidationProblemDetails AssertValidationProblem<T>(ActionResult<T> result,
            ValidationErrorMessages message) where T : class
        {
            var validationProblem = AssertValidationProblemIsBadDetails(result);
            return AssertValidationProblemDetails(validationProblem, message);
        }

        private static ValidationProblemDetails AssertValidationProblemIsBadDetails(ActionResult result)
        {
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            var badRequestObjectResult = result as BadRequestObjectResult;
            return AssertValidationProblemHasDetails(badRequestObjectResult);
        }

        private static ValidationProblemDetails AssertValidationProblemIsBadDetails<T>(ActionResult<T> result)
            where T : class
        {
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
            var badRequestObjectResult = result.Result as BadRequestObjectResult;
            return AssertValidationProblemHasDetails(badRequestObjectResult);
        }

        private static ValidationProblemDetails AssertValidationProblemHasDetails(
            BadRequestObjectResult badRequestObjectResult)
        {
            Assert.IsAssignableFrom<ValidationProblemDetails>(badRequestObjectResult?.Value);
            var validationProblemDetails = badRequestObjectResult.Value as ValidationProblemDetails;
            return validationProblemDetails;
        }

        private static ValidationProblemDetails AssertValidationProblemDetails(
            ValidationProblemDetails validationProblem, ValidationErrorMessages message)
        {
            Assert.True(validationProblem.Errors.ContainsKey(string.Empty));
            Assert.Contains(ValidationResult(message).ErrorMessage, validationProblem.Errors[string.Empty]);
            return validationProblem;
        }
    }
}