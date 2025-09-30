#nullable enable
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.AllowedValueValidator;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ValidationProblemViewModelTestExtensions
{
    public static ErrorViewModel AssertHasEmailError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.EmailValidator
        );

    public static ErrorViewModel AssertHasGreaterThanOrEqualError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.GreaterThanOrEqualValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasGreaterThanError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.GreaterThanValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LengthValidator
        );

    public static ErrorViewModel AssertHasMinimumLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int minLength)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.MinimumLengthValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(minLength, errorDetail["minLength"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasMaximumLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int maxLength)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.MaximumLengthValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(maxLength, errorDetail["maxLength"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasLessThanOrEqualError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LessThanOrEqualValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasLessThanError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LessThanValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasNotEqualError<T>(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        T comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.NotEqualValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].Deserialize<T>());

        return error;
    }

    public static ErrorViewModel AssertHasNotEmptyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.NotEmptyValidator
        );

    public static ErrorViewModel AssertHasNotNullError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.NotNullValidator
        );

    public static ErrorViewModel AssertHasPredicateError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.PredicateValidator
        );

    public static ErrorViewModel AssertHasAsyncPredicateError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.AsyncPredicateValidator
        );

    public static ErrorViewModel AssertHasRegularExpressionError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.RegularExpressionValidator
        );

    public static ErrorViewModel AssertHasEqualError<T>(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        T comparisonValue)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.EqualValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(comparisonValue, errorDetail["comparisonValue"].Deserialize<T>());

        return error;
    }

    public static ErrorViewModel AssertHasExactLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int minLength,
        int maxLength)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.ExactLengthValidator
        );


        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(minLength, errorDetail["minLength"].GetInt32());
        Assert.Equal(maxLength, errorDetail["maxLength"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasInclusiveBetweenError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int from,
        int to)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.InclusiveBetweenValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(from, errorDetail["from"].GetInt32());
        Assert.Equal(to, errorDetail["to"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasExclusiveBetweenError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        int from,
        int to)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.ExclusiveBetweenValidator
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(from, errorDetail["from"].GetInt32());
        Assert.Equal(to, errorDetail["to"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasCreditCardError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.CreditCardValidator
        );

    public static ErrorViewModel AssertHasScalePrecisionError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.ScalePrecisionValidator
        );

    public static ErrorViewModel AssertHasEmptyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.EmptyValidator
        );

    public static ErrorViewModel AssertHasNullError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.NullValidator
        );

    public static ErrorViewModel AssertHasEnumError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.EnumValidator
        );

    public static ErrorViewModel AssertHasAllowedValueError<TValue>(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        TValue? value)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.AllowedValue.Code
        );

        var errorDetail = error.GetDetail<AllowedErrorDetail<TValue>>();

        Assert.Equal(value, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasAllowedValueError<TValue>(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        TValue? value,
        IEnumerable<TValue> allowed)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.AllowedValue.Code
        );

        var errorDetail = error.GetDetail<AllowedErrorDetail<TValue>>();

        Assert.Equal(value, errorDetail.Value);
        Assert.Equal(allowed, errorDetail.Allowed);

        return error;
    }

    public static ErrorViewModel AssertHasNotEmptyBodyError(
        this ValidationProblemViewModel validationProblem)
    {
        return validationProblem.AssertHasError(
            expectedPath: null,
            expectedCode: ValidationMessages.NotEmptyBody.Code
        );
    }

    public static ErrorViewModel AssertHasInvalidInputError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        return validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.InvalidValue.Code
        );
    }

    public static ErrorViewModel AssertHasRequiredValueError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        return validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.RequiredValue.Code
        );
    }

    public static ErrorViewModel AssertHasInvalidValueError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        return validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.InvalidValue.Code
        );
    }

    public static ErrorViewModel AssertHasUnknownFieldError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        return validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.UnknownField.Code
        );
    }

    public static ErrorViewModel AssertHasError(
        this ValidationProblemViewModel validationProblem,
        string? expectedPath,
        string expectedCode,
        string? expectedMessage = null,
        object? expectedDetail = null)
    {
        Predicate<ErrorViewModel> predicate = error =>
        {
            if (expectedMessage is not null && error.Message != expectedMessage)
            {
                return false;
            }

            if (expectedDetail is not null && (error.Detail is null || !error.Detail.Equals(expectedDetail)))
            {
                return false;
            }

            return error.Path == expectedPath && error.Code == expectedCode;
        };

        Assert.Contains(validationProblem.Errors, predicate);

        return validationProblem.Errors.First(new Func<ErrorViewModel, bool>(predicate));
    }

    public static void AssertDoesNotHaveError(
        this ValidationProblemViewModel validationProblem,
        string? expectedPath,
        string expectedCode,
        string? expectedMessage = null,
        object? expectedDetail = null)
    {
        Predicate<ErrorViewModel> predicate = error =>
        {
            if (expectedMessage is not null && error.Message != expectedMessage)
            {
                return false;
            }

            if (expectedDetail is not null && (error.Detail is null || !error.Detail.Equals(expectedDetail)))
            {
                return false;
            }

            return error.Path == expectedPath && error.Code == expectedCode;
        };

        Assert.DoesNotContain(validationProblem.Errors, predicate);
    }

    public static void AssertHasErrors(
        this ValidationProblemViewModel validationProblem,
        List<ErrorViewModel> expectedErrors)
    {
        var errors = validationProblem.Errors.ToList();
        AssertHasErrors(errors, expectedErrors);
    }

    public static void AssertDoesNotContainErrors(
        List<ErrorViewModel> errors,
        List<ErrorViewModel> expectedNotToContain)
    {
        var matchedErrors = errors
            .Where(error => expectedNotToContain.Any(expected => ErrorsMatch(error, expected)))
            .ToArray();

        if (matchedErrors.Length == 0)
        {
            return;
        }

        var matchedErrorMessages = matchedErrors
            .Select(e => e.Message)
            .JoinToString('\n');

        Assert.Fail($"""
            Error message(s) found in expectedMissingErrors that should not be present:
            {matchedErrorMessages}
            """);

        return;
        static bool ErrorsMatch(ErrorViewModel current, ErrorViewModel expected) =>
            current.Code == expected.Code &&
            current.Path == expected.Path;
    }

    public static void AssertHasErrors(
        List<ErrorViewModel> errors,
        List<ErrorViewModel> expectedErrors)
    {
        List<ErrorViewModel> notFoundErrors = [];

        foreach (var error in errors)
        {
            var foundError = expectedErrors.Find(expected => expected.Message == error.Message);
            if (foundError == null)
            {
                notFoundErrors.Add(error);
                continue;
            }

            Assert.Equal(foundError.Code, error.Code);
            Assert.Equal(foundError.Path, error.Path);

            expectedErrors.Remove(foundError);
        }

        var assertFailMessage = string.Empty;

        if (notFoundErrors.Count != 0)
        {
            var notFoundErrorMessages = notFoundErrors
                .Select(e => e.Message)
                .ToList()
                .JoinToString('\n');
            assertFailMessage += $"Error message(s) not found in expectedErrors:\n{notFoundErrorMessages}\n";
        }

        if (expectedErrors.Count != 0)
        {
            var expectedErrorMessages = expectedErrors
                .Select(e => e.Message)
                .ToList()
                .JoinToString('\n');
            assertFailMessage += $"expectedErrors message(s) were not in the response:\n{expectedErrorMessages}\n";
        }

        if (assertFailMessage != string.Empty)
        {
            Assert.Fail(assertFailMessage);
        }
    }

    public static ErrorViewModel AssertHasGlobalError(
        this ValidationProblemViewModel validationProblem,
        Enum expectedCode) =>
        validationProblem.AssertHasGlobalError(expectedCode.ToString());

    public static ErrorViewModel AssertHasGlobalError(
        this ValidationProblemViewModel validationProblem,
        string expectedCode) =>
        validationProblem.AssertHasError(expectedPath: null, expectedCode);

    private static ErrorViewModel AssertHasFluentValidationError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string expectedKey)
    {
        var expectedCode = expectedKey.Replace("Validator", "");

        return AssertHasError(validationProblem, expectedPath, expectedCode);
    }
}
