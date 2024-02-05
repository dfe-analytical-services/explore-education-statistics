#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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

            return result.Value!;
        }

        public static void AssertNotFoundResult<T>(this ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        public static void AssertNotFoundResult(this ActionResult result)
        {
            Assert.IsAssignableFrom<NotFoundResult>(result);
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

        public static void AssertValidationProblem<T>(this ActionResult<T> result, params Enum[] expectedErrorCodes)
        {
            Assert.NotNull(result.Result);
            AssertBadRequestWithValidationErrors(result.Result, expectedErrorCodes);
        }

        public static void AssertValidationProblem(this ActionResult result, params Enum[] expectedErrorCodes)
        {
            AssertBadRequestWithValidationErrors(result, expectedErrorCodes);
        }

        public static void AssertValidationProblem(this ActionResult result, string expectedError)
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

        private static void AssertBadRequestWithValidationErrors(this object result, params Enum[] expectedErrorCodes)
        {
            var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            var validationProblem = Assert.IsAssignableFrom<ValidationProblemViewModel>(badRequest.Value);

            Assert.NotNull(validationProblem);

            var errorCodes = validationProblem.Errors
                .Select(error => error.Code)
                .ToList();

            Assert.Equal(expectedErrorCodes.Length, errorCodes.Count);

            expectedErrorCodes.ForEach(errorCode =>
                Assert.Contains(errorCode.ToString(), errorCodes));
        }
    }
}
