using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class LocationStringValidators
{
    private const string ExpectedFormat = "{level}|{property}|{value}";

    public static IRuleBuilderOptionsConditions<T, string> LocationString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .Custom(
                (value, context) =>
                {
                    if (value.IsNullOrWhitespace())
                    {
                        return;
                    }

                    if (!HasValidFormat(value))
                    {
                        context.AddFailure(
                            message: ValidationMessages.LocationFormat,
                            detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                        );
                        return;
                    }

                    var parts = value.Split('|');
                    var level = parts[0];
                    var property = parts[1];

                    if (!HasAllowedLevel(level))
                    {
                        context.AddFailure(
                            message: ValidationMessages.LocationAllowedLevel,
                            detail: new LocationAllowedLevelErrorDetail(
                                Value: level,
                                AllowedLevels: EnumUtil.GetEnumValues<GeographicLevel>().Order().ToList()
                            )
                        );
                        return;
                    }

                    if (!HasAllowedProperty(level: level, property: property))
                    {
                        context.AddFailure(
                            message: ValidationMessages.LocationAllowedProperty,
                            detail: new LocationAllowedPropertyErrorDetail(property, GetAllowedProperties(level: level))
                        );
                        return;
                    }

                    var queryLocation = IDataSetQueryLocation.Parse(value);

                    var validator = queryLocation.CreateValidator();
                    var result = validator.Validate(new ValidationContext<IDataSetQueryLocation>(queryLocation));

                    if (result.IsValid)
                    {
                        return;
                    }

                    foreach (var error in result.Errors)
                    {
                        var propertyName = error.PropertyName.ToLowerFirst();

                        switch (error.ErrorCode)
                        {
                            case FluentValidationKeys.NotEmptyValidator:
                                error.ErrorCode = ValidationMessages.LocationValueNotEmpty.Code;
                                error.ErrorMessage = context
                                    .MessageFormatter.AppendArgument("Property", propertyName)
                                    .BuildMessage(ValidationMessages.LocationValueNotEmpty.Message);
                                break;

                            case FluentValidationKeys.MaximumLengthValidator:
                                error.ErrorCode = ValidationMessages.LocationValueMaxLength.Code;
                                error.ErrorMessage = context
                                    .MessageFormatter.AppendArgument("Property", propertyName)
                                    .AppendArgument("MaxLength", error.FormattedMessagePlaceholderValues["MaxLength"])
                                    .BuildMessage(ValidationMessages.LocationValueMaxLength.Message);
                                break;
                        }

                        error.FormattedMessagePlaceholderValues["Property"] = propertyName;
                        error.PropertyName = context.PropertyPath;

                        context.AddFailure(error);
                    }
                }
            );
    }

    // Note this regex only does basic checks on delimiters to allow other
    // validations to provide more specific error messages on what is invalid.
    [GeneratedRegex(@"^[^|]*\|[^|]+\|[^|]*$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasAllowedLevel(string level) => EnumUtil.TryGetFromEnumValue<GeographicLevel>(level, out _);

    private static bool HasAllowedProperty(string level, string property) =>
        GetAllowedProperties(level).Contains(property);

    private static string[] GetAllowedProperties(string level)
    {
        var geographicLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(level);

        string[] properties = geographicLevel switch
        {
            GeographicLevel.LocalAuthority =>
            [
                nameof(DataSetQueryLocationId.Id),
                nameof(DataSetQueryLocationLocalAuthorityCode.Code),
                nameof(DataSetQueryLocationLocalAuthorityOldCode.OldCode),
            ],
            GeographicLevel.School =>
            [
                nameof(DataSetQueryLocationId.Id),
                nameof(LocationSchoolOptionMeta.Urn),
                nameof(LocationSchoolOptionMeta.LaEstab),
            ],
            GeographicLevel.Provider => [nameof(DataSetQueryLocationId.Id), nameof(LocationProviderOptionMeta.Ukprn)],
            GeographicLevel.RscRegion => [nameof(DataSetQueryLocationId.Id)],
            _ => [nameof(DataSetQueryLocationId.Id), nameof(DataSetQueryLocationCode.Code)],
        };

        return properties.Select(property => property.ToLowerFirst()).ToArray();
    }
}
