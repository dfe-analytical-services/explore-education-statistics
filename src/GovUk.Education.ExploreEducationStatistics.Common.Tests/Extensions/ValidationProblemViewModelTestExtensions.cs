#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.GreaterThanOrEqualValidator
        );

    public static ErrorViewModel AssertHasGreaterThanError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.GreaterThanValidator
        );

    public static ErrorViewModel AssertHasLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LengthValidator
        );

    public static ErrorViewModel AssertHasMinimumLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.MinimumLengthValidator
        );

    public static ErrorViewModel AssertHasMaximumLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.MaximumLengthValidator
        );

    public static ErrorViewModel AssertHasLessThanOrEqualError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LessThanOrEqualValidator
        );

    public static ErrorViewModel AssertHasLessThanError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.LessThanValidator
        );

    public static ErrorViewModel AssertHasNotEqualError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.NotEqualValidator
        );

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

    public static ErrorViewModel AssertHasEqualError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.EqualValidator
        );

    public static ErrorViewModel AssertHasExactLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.ExactLengthValidator
        );

    public static ErrorViewModel AssertHasInclusiveBetweenError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.InclusiveBetweenValidator
        );

    public static ErrorViewModel AssertHasExclusiveBetweenError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath)
        => validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: FluentValidationKeys.ExclusiveBetweenValidator
        );

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
    public static ErrorViewModel AssertHasAllowedValueError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string? value,
        IEnumerable<string> allowed)
    {
        var error = validationProblem.AssertHasFluentValidationError(
            expectedPath: expectedPath,
            expectedKey: ValidationMessages.AllowedValue.Code
        );

        var errorDetail = GetErrorDetail(error);

        Assert.Equal(2, errorDetail.Count);
        Assert.Equal(value, errorDetail[nameof(AllowedErrorDetail<object>.Value).ToLowerFirst()].GetString());
        Assert.Equal(
            allowed,
            errorDetail[nameof(AllowedErrorDetail<object>.Allowed).ToLowerFirst()]
                .EnumerateArray()
                .Select(e => e.GetString()!));

        return error;
    }

    public static ErrorViewModel AssertHasError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string expectedCode)
    {
        Predicate<ErrorViewModel> predicate = error => error.Path == expectedPath && error.Code == expectedCode;

        Assert.Contains(validationProblem.Errors, predicate);

        return validationProblem.Errors.First(new Func<ErrorViewModel, bool>(predicate));
    }

    private static ErrorViewModel AssertHasFluentValidationError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string expectedKey)
    {
        var expectedCode = expectedKey.Replace("Validator", "");

        return AssertHasError(validationProblem, expectedPath, expectedCode);
    }

    private static Dictionary<string, JsonElement> GetErrorDetail(ErrorViewModel error)
    {
        var errorDetailJson = Assert.IsType<JsonElement>(error.Detail);
        var errorDetail = errorDetailJson.Deserialize<Dictionary<string, JsonElement>>();

        Assert.NotNull(errorDetail);
        return errorDetail;
    }
}
