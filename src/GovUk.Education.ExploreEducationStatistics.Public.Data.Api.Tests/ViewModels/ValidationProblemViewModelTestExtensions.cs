using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;

public static class ValidationProblemViewModelTestExtensions
{
    public static ErrorViewModel AssertHasIndicatorsNotFoundError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
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
        string value
    )
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
        string level
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationAllowedLevel.Code
        );

        var errorDetail = error.GetDetail<LocationAllowedLevelErrorDetail>();

        Assert.Equal(level, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasLocationAllowedPropertyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string property,
        IEnumerable<string> allowedProperties
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationAllowedProperty.Code
        );

        var errorDetail = error.GetDetail<LocationAllowedPropertyErrorDetail>();

        Assert.Equal(property, errorDetail.Value);
        Assert.Equal(allowedProperties, errorDetail.AllowedProperties);

        return error;
    }

    public static ErrorViewModel AssertHasLocationValueNotEmptyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string property
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationValueNotEmpty.Code
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(property, errorDetail["property"].GetString());

        return error;
    }

    public static ErrorViewModel AssertHasLocationValueMaxLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string property,
        int maxLength
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationValueMaxLength.Code
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal(property, errorDetail["property"].GetString());
        Assert.Equal(maxLength, errorDetail["maxLength"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasSortFormatError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string? value
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFormat.Code
        );

        var errorDetail = error.GetDetail<FormatErrorDetail>();

        Assert.Equal(value, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasSortFieldNotEmptyError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFieldNotEmpty.Code
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal("field", errorDetail["property"].GetString());

        return error;
    }

    public static ErrorViewModel AssertHasSortFieldMaxLengthError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string field,
        int maxLength = 40
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortFieldMaxLength.Code
        );

        var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

        Assert.Equal("field", errorDetail["property"].GetString());
        Assert.Equal(field, errorDetail["value"].GetString());
        Assert.Equal(maxLength, errorDetail["maxLength"].GetInt32());

        return error;
    }

    public static ErrorViewModel AssertHasSortDirectionError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        string direction
    )
    {
        var error = validationProblem.AssertHasError(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.SortDirection.Code
        );

        var errorDetail = error.GetDetail<AllowedValueValidator.AllowedErrorDetail<string>>();

        Assert.Equal(direction, errorDetail.Value);

        return error;
    }

    public static ErrorViewModel AssertHasSortFieldsNotFoundError(
        this ValidationProblemViewModel validationProblem,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
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
        IEnumerable<DataSetQuerySort> notFoundItems
    )
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
        string value
    )
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
        string period
    )
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
        string period
    )
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
        string code
    )
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
