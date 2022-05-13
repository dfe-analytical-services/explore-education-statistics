#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ActionResultTestUtils
    {
        public static T AssertOkResult<T>(this ActionResult<T> result, T? expectedValue = null) where T : class
        {
            Assert.IsAssignableFrom<ActionResult<T>>(result);
            Assert.IsAssignableFrom<T>(result.Value);

            if (expectedValue != null)
            {
                Assert.Equal(expectedValue, result.Value);
            }

            return result.Value;
        }

        public static void AssertNotFoundResult<T>(this ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        public static void AssertNoContent(this ActionResult result)
        {
            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        public static void AssertForbidden(this ActionResult result)
        {
            Assert.IsAssignableFrom<ForbidResult>(result);
        }

        public static void AssertForbidden<T>(this ActionResult<T> result)
        {
            Assert.IsAssignableFrom<ForbidResult>(result);
        }

        public static void AssertAccepted(this ActionResult result)
        {
            Assert.IsAssignableFrom<AcceptedResult>(result);
        }

        public static void AssertBadRequest<T>(this ActionResult<T> result, params Enum[] expectedValidationErrors)
        {
            AssertBadRequestWithValidationErrors(result.Result, expectedValidationErrors);
        }

        public static void AssertBadRequest(this ActionResult result, params Enum[] expectedValidationErrors)
        {
            AssertBadRequestWithValidationErrors(result, expectedValidationErrors);
        }

        public static void AssertBadRequest(this ActionResult result, string expectedError)
        {
            var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            var error = Assert.IsAssignableFrom<string>(badRequest.Value);
            Assert.Equal(expectedError, error);
        }

        public static void AssertNotModified<T>(this ActionResult<T> result)
        {
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result.Result);
            Assert.Equal(StatusCodes.Status304NotModified, statusCodeResult.StatusCode);
        }

        private static void AssertBadRequestWithValidationErrors(this object result, params Enum[] expectedValidationErrors)
        {
            var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            var validationProblem = Assert.IsAssignableFrom<ValidationProblemDetails>(badRequest.Value);

            Assert.NotNull(validationProblem);
            Assert.True(validationProblem.Errors.ContainsKey(string.Empty));

            var globalErrors = validationProblem.Errors[string.Empty];

            Assert.Equal(expectedValidationErrors.Length, globalErrors.Length);

            expectedValidationErrors.ForEach(message =>
                Assert.Contains(message.ToString(), globalErrors));
        }
    }
}
