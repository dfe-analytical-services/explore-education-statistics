using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class LocationStringValidators
{
    private const string ExpectedFormat = "{level}|{property}|{value}";

    public static IRuleBuilderOptionsConditions<T, string> LocationString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Custom((value, context) =>
        {
            if (!HasValidFormat(value))
            {
                context.AddFailure(
                    message: ValidationMessages.LocationFormat,
                    detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                );
                return;
            }

            var location = new ParsedLocation(value);

            if (!HasAllowedLevel(location))
            {
                context.AddFailure(
                    message: ValidationMessages.LocationAllowedLevel,
                    detail: new AllowedLevelErrorDetail(location.String)
                );
                return;
            }

            if (!HasAllowedProperty(location))
            {
                context.AddFailure(
                    message: ValidationMessages.LocationAllowedProperty,
                    detail: new AllowedPropertyErrorDetail(location.String, GetAllowedProperties(location))
                );
            }

            if (!HasValidValueLength(location))
            {
                var maxLength = GetMaxLength(location);

                context.AddFailure(
                    message: ValidationMessages.LocationMaxValueLength,
                    detail: new MaxValueLengthErrorDetail(location.String, maxLength),
                    messagePlaceholders: new Dictionary<string, object>
                    {
                        {
                            "MaxLength", maxLength
                        }
                    }
                );
            }
        });
    }

    [GeneratedRegex(@"^[A-Z]{2,4}\|[A-Za-z]+\|\w+( \/ )?\w+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasAllowedLevel(ParsedLocation location)
    {
        return EnumUtil.TryGetFromEnumValue<GeographicLevel>(location.Level, out _);
    }

    private static bool HasAllowedProperty(ParsedLocation location)
    {
        return GetAllowedProperties(location).Contains(location.Property);
    }

    private static bool HasValidValueLength(ParsedLocation location)
    {
        return location.Value.Length <= GetMaxLength(location);
    }

    private static string[] GetAllowedProperties(ParsedLocation location)
    {
        var geographicLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(location.Level);

        string[] properties = geographicLevel switch
        {
            GeographicLevel.LocalAuthority =>
            [
                "id",
                nameof(LocationLocalAuthorityOptionMeta.Code),
                nameof(LocationLocalAuthorityOptionMeta.OldCode)
            ],
            GeographicLevel.School =>
            [
                "id",
                nameof(LocationSchoolOptionMeta.Urn),
                nameof(LocationSchoolOptionMeta.LaEstab)
            ],
            GeographicLevel.Provider =>
            [
                "id",
                nameof(LocationProviderOptionMeta.Ukprn)
            ],
            GeographicLevel.RscRegion => ["id"],
            _ => ["id", "code"]
        };

        return properties
            .Select(property => property.ToLowerFirst())
            .ToArray();
    }

    private static int GetMaxLength(ParsedLocation location)
    {
        var geographicLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(location.Level);
        var property = location.Property.ToUpperFirst();

        return geographicLevel switch
        {
            GeographicLevel.LocalAuthority => property switch
            {
                nameof(LocationLocalAuthorityOptionMeta.Code) => 25,
                nameof(LocationLocalAuthorityOptionMeta.OldCode) => 10,
                _ => 10
            },
            GeographicLevel.School => property switch
            {
                nameof(LocationSchoolOptionMeta.Urn) => 6,
                nameof(LocationSchoolOptionMeta.LaEstab) => 7,
                _ => 10
            },
            GeographicLevel.Provider => property switch
            {
                nameof(LocationProviderOptionMeta.Ukprn) => 8,
                _ => 10
            },
            _ => property switch
            {
                nameof(LocationCodedOptionMeta.Code) => 25,
                _ => 10
            }
        };
    }

    public record AllowedLevelErrorDetail(string Value) : InvalidErrorDetail<string>(Value)
    {
        public IReadOnlyList<string> Allowed => EnumUtil.GetEnumValues<GeographicLevel>().Order().ToList();
    }

    public record AllowedPropertyErrorDetail(string Value, IEnumerable<string> Allowed)
        : InvalidErrorDetail<string>(Value);

    public record MaxValueLengthErrorDetail(string Value, int MaxLength) : InvalidErrorDetail<string>(Value);

    private record ParsedLocation
    {
        public string String { get; init; }

        public string Level { get; init; }

        public string Property { get; init; }

        public string Value { get; init; }


        public ParsedLocation(string locationString)
        {
            var parts = locationString.Split('|');

            String = locationString;
            Level = parts[0];
            Property = parts[1];
            Value = parts[2];
        }
    }
}
