#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
