#nullable enable
using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ActionResultTestUtils
{
    public static T AssertOkObjectResult<T>(this IActionResult result, T? expectedValue = null)
        where T : class
    {
        var okObjectResult = Assert.IsAssignableFrom<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<T>(okObjectResult.Value);

        Assert.NotNull(value);

        if (expectedValue != null)
        {
            Assert.Equal(expectedValue, value);
        }

        return value;
    }

    public static void AssertOkResult(this ActionResult result) => Assert.IsType<OkResult>(result);

    public static T AssertOkResult<T>(this ActionResult<T> result, T? expectedValue = null)
        where T : class
    {
        Assert.IsAssignableFrom<ActionResult<T>>(result);
        var value = Assert.IsAssignableFrom<T>(result.Value);

        if (expectedValue != null)
        {
            Assert.Equal(expectedValue, value);
        }

        return value;
    }

    public static T AssertObjectResult<T>(
        this IActionResult result,
        HttpStatusCode expectedStatusCode,
        T? expectedValue = null
    )
        where T : class
    {
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal((int)expectedStatusCode, objectResult.StatusCode);
        var value = Assert.IsAssignableFrom<T>(objectResult.Value);

        if (expectedValue != null)
        {
            Assert.Equal(expectedValue, value);
        }

        return value;
    }

    public static void AssertNotFoundResult<T>(this ActionResult<T> result)
        where T : class
    {
        Assert.IsAssignableFrom<NotFoundResult>(result.Result);
    }

    public static void AssertNotFoundResult(this IActionResult result)
    {
        Assert.IsAssignableFrom<NotFoundResult>(result);
    }

    public static void AssertNoContent(this IActionResult result)
    {
        Assert.IsAssignableFrom<NoContentResult>(result);
    }

    public static void AssertForbidden(this IActionResult result)
    {
        Assert.IsAssignableFrom<ForbidResult>(result);
    }

    public static void AssertForbidden<T>(this ActionResult<T> result)
    {
        Assert.IsAssignableFrom<ForbidResult>(result);
    }

    public static void AssertAccepted(this IActionResult result)
    {
        Assert.IsAssignableFrom<AcceptedResult>(result);
    }

    public static void AssertInternalServerError(this IActionResult result)
    {
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    public static void AssertInternalServerError<T>(this ActionResult<T> result)
    {
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    public static void AssertValidationProblem<T>(this ActionResult<T> result, params Enum[] expectedErrorCodes)
    {
        Assert.NotNull(result.Result);
        AssertBadRequestWithValidationErrors(result.Result, expectedErrorCodes);
    }

    public static void AssertValidationProblem(this IActionResult result, params Enum[] expectedErrorCodes)
    {
        AssertBadRequestWithValidationErrors(result, expectedErrorCodes);
    }

    public static void AssertValidationProblem(this IActionResult result, string expectedError)
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

    public static ValidationProblemViewModel AssertBadRequestWithValidationProblem(this IActionResult result)
    {
        var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        var validationProblem = Assert.IsAssignableFrom<ValidationProblemViewModel>(badRequest.Value);
        return validationProblem;
    }

    public static void AssertBadRequestWithValidationErrors(
        this IActionResult result,
        List<ErrorViewModel> expectedErrors
    )
    {
        var validationProblem = result.AssertBadRequestWithValidationProblem();

        Assert.NotNull(validationProblem);

        validationProblem.AssertHasErrors(expectedErrors);
    }

    public static ValidationProblemViewModel AssertNotFoundWithValidationProblem<TEntity, TId>(
        this IActionResult result,
        TId expectedId,
        string expectedPath
    )
    {
        var notFound = Assert.IsAssignableFrom<NotFoundObjectResult>(result);
        var validationProblem = Assert.IsAssignableFrom<ValidationProblemViewModel>(notFound.Value);

        validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: "NotFound",
            expectedMessage: $"{typeof(TEntity).Name} not found",
            expectedDetail: new InvalidErrorDetail<TId>(expectedId)
        );

        return validationProblem;
    }

    private static void AssertBadRequestWithValidationErrors(
        this IActionResult result,
        params Enum[] expectedErrorCodes
    )
    {
        var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        var validationProblem = Assert.IsAssignableFrom<ValidationProblemViewModel>(badRequest.Value);

        Assert.NotNull(validationProblem);

        var errorCodes = validationProblem.Errors.Select(error => error.Code).ToList();

        Assert.Equal(expectedErrorCodes.Length, errorCodes.Count);

        expectedErrorCodes.ForEach(errorCode => Assert.Contains(errorCode.ToString(), errorCodes));
    }

    public static FileStreamResult AssertFileStreamResult(this IActionResult result, FileStreamResult expectedResult)
    {
        var fileStreamResult = Assert.IsAssignableFrom<FileStreamResult>(result);
        Assert.Equal(expectedResult, fileStreamResult);
        return fileStreamResult;
    }

    public static FileStreamResult AssertFileStreamResult(
        this IActionResult result,
        string expectedFilename,
        string expectedContentType,
        Stream? expectedStream = null
    )
    {
        var fileStreamResult = Assert.IsAssignableFrom<FileStreamResult>(result);

        Assert.Equal(expectedFilename, fileStreamResult.FileDownloadName);
        Assert.Equal(expectedContentType, fileStreamResult.ContentType);

        if (expectedStream != null)
        {
            Assert.Equal(expectedStream, fileStreamResult.FileStream);
        }

        return fileStreamResult;
    }
}
