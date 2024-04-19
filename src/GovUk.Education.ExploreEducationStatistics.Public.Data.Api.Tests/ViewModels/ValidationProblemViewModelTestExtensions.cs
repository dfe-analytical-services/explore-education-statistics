using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;

public static class ValidationProblemViewModelTestExtensions
{
    public static ErrorViewModel AssertHasIndicatorsNotFoundError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        IEnumerable<string> notFoundItems)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.IndicatorsNotFound.Code
        );

        var errorDetail = error.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, errorDetail.Items);

        return error;
    }

        public static ErrorViewModel AssertHasLocationFormatError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string value)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationFormat.Code
        );

        var errorDetail = error.GetDetail<FormatErrorDetail>();

        Assert.Equal(value, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasLocationAllowedLevelError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string level)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationAllowedLevel.Code
        );

        var errorDetail = error.GetDetail<LocationStringValidators.AllowedLevelErrorDetail>();

        Assert.Equal(level, errorDetail.Value.Level);

        return error;
    }

    public static ErrorViewModel AssertHasLocationAllowedPropertyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string property,
        IEnumerable<string> allowedProperties)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationAllowedProperty.Code
        );

        var errorDetail = error.GetDetail<LocationStringValidators.AllowedPropertyErrorDetail>();

        Assert.Equal(property, errorDetail.Value.Property);
        Assert.Equal(allowedProperties, errorDetail.AllowedProperties);

        return error;
    }

    public static ErrorViewModel AssertHasLocationMaxValueLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string value,
        int maxValueLength)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationMaxValueLength.Code
        );

        var errorDetail = error.GetDetail<LocationStringValidators.MaxValueLengthErrorDetail>();

        Assert.Equal(value, errorDetail.Value.Value);
        Assert.Equal(maxValueLength, errorDetail.MaxValueLength);

        return error;
    }

    public static ErrorViewModel AssertHasSortFormatError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string? value)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFormat.Code
        );

        var errorDetail = error.GetDetail<FormatErrorDetail>();

        Assert.Equal(value, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasSortMaxFieldLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string field,
        int maxFieldLength = 40)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortMaxFieldLength.Code
        );

        var errorDetail = error.GetDetail<SortStringValidators.MaxFieldLengthErrorDetail>();

        Assert.Equal(field, errorDetail.Value.Field);
        Assert.Equal(maxFieldLength, errorDetail.MaxFieldLength);

        return error;
    }

    public static ErrorViewModel AssertHasSortDirectionError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string direction)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortDirection.Code
        );

        var errorDetail = error.GetDetail<SortStringValidators.DirectionErrorDetail>();

        Assert.Equal(direction, errorDetail.Value.Direction);

        return error;
    }

    public static ErrorViewModel AssertHasSortFieldsNotFoundError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        IEnumerable<string> notFoundItems)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFieldsNotFound.Code
        );

        var errorDetail = error.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, errorDetail.Items);

        return error;
    }

    public static ErrorViewModel AssertHasSortFieldsNotFoundError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        IEnumerable<DataSetQuerySort> notFoundItems)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFieldsNotFound.Code
        );

        var errorDetail = error.GetDetail<NotFoundItemsErrorDetail<DataSetQuerySort>>();

        Assert.Equal(notFoundItems, errorDetail.Items);

        return error;
    }

    public static ErrorViewModel AssertHasTimePeriodFormatError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string value)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodFormat.Code
        );

        var errorDetail = error.GetDetail<FormatErrorDetail>();

        Assert.Equal(value, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasTimePeriodInvalidYearError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string period)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodInvalidYear.Code
        );

        var errorDetail = error.GetDetail<InvalidErrorDetail<string>>();

        Assert.Equal(period, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasTimePeriodYearRangeError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string period)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodInvalidYearRange.Code
        );

        var errorDetail = error.GetDetail<InvalidErrorDetail<string>>();

        Assert.Equal(period, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasTimePeriodAllowedCodeError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string code)
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodAllowedCode.Code
        );

        var errorDetail = error.GetDetail<TimePeriodAllowedCodeErrorDetail>();

        Assert.Equal(code, errorDetail.Value);

        return error;
    }
}
