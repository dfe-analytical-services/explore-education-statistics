#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        var error = validationProblem.AssertHasError(
            expectedPath: null,
            expectedCode: ValidationMessages.NotEmptyBody.Code
        );

        return error;
    }

    public static ErrorViewModel AssertHasInvalidInputError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.InvalidInput.Code
        );

        return error;
    }

    public static ErrorViewModel AssertHasRequiredFieldError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.RequiredField.Code
        );

        return error;
    }

    public static ErrorViewModel AssertHasError(
        this ValidationProblemViewModel validationProblem,
        string? expectedPath,
        string expectedCode)
    {
        Predicate<ErrorViewModel> predicate = error => error.Path == expectedPath && error.Code == expectedCode;

        Assert.Contains(validationProblem.Errors, predicate);

        return validationProblem.Errors.First(new Func<ErrorViewModel, bool>(predicate));
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
