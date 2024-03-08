using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class LocationStringValidators
{
    public static IRuleBuilderOptionsConditions<T, string> LocationString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Custom((value, context) => Validate([value], context, usePluralMessage: false));
    }

    public static IRuleBuilderOptionsConditions<T, IEnumerable<string>> OnlyLocationStrings<T>(
        this IRuleBuilder<T, IEnumerable<string>> rule)
    {
        return rule.Custom((values, context) => Validate([..values], context));
    }

    private static void Validate<T>(
        IList<string> values,
        ValidationContext<T> context,
        bool usePluralMessage = true)
    {
        var distinctValues = values.Distinct().ToList();

        var invalidFormats = distinctValues
            .Where(value => !HasValidFormat(value))
            .ToList();

        if (invalidFormats.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.LocationFormat,
                detail: new FormatErrorDetail(invalidFormats),
                usePluralMessage: usePluralMessage
            );
            return;
        }

        var locations = distinctValues
            .Select(value => new ParsedLocation(value))
            .ToList();

        var invalidLevels = new List<string>();
        var invalidProperties = new List<InvalidPropertyItem>();
        var invalidValueLengths = new List<string>();

        foreach (var location in locations)
        {
            if (!HasAllowedLevel(location))
            {
                invalidLevels.Add(location.String);
                continue;
            }

            if (!HasAllowedProperty(location))
            {
                invalidProperties.Add(new InvalidPropertyItem
                {
                    Value = location.String,
                    Allowed = GetAllowedProperties(location)
                });
                continue;
            }

            if (!HasValidValueLength(location))
            {
                invalidValueLengths.Add(location.String);
            }
        }

        if (invalidLevels.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.LocationAllowedLevel,
                detail: new AllowedLevelErrorDetail(invalidLevels),
                usePluralMessage: usePluralMessage
            );
            return;
        }

        if (invalidProperties.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.LocationAllowedProperty,
                detail: new AllowedPropertyErrorDetail(invalidProperties),
                usePluralMessage: usePluralMessage
            );
        }

        if (invalidValueLengths.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.LocationMaxValueLength,
                detail: new MaxValueLengthDetail(invalidValueLengths),
                messagePlaceholders: new Dictionary<string, object>
                {
                    { "MaxLength", MaxValueLength }
                },
                usePluralMessage: usePluralMessage
            );
        }
    }

    private const int MaxValueLength = 25;

    [GeneratedRegex(@"^[A-Z]{2,4}\|[A-Za-z]+\|[\w\s/]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
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
        return location.Value.Length <= MaxValueLength;
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

    public record FormatErrorDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid)
    {
        public string ExpectedFormat => "{period}|{code}";
    }

    public record AllowedLevelErrorDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid)
    {
        public IReadOnlyList<string> Allowed => EnumUtil.GetEnumValues<GeographicLevel>().Order().ToList();
    }

    public record AllowedPropertyErrorDetail(IEnumerable<InvalidPropertyItem> Invalid)
        : InvalidItemsErrorDetail<InvalidPropertyItem>(Invalid);

    public record InvalidPropertyItem
    {
        public required string Value { get; init; }

        public required string[] Allowed { get; init; }
    }

    public record MaxValueLengthDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid)
    {
        public int MaxLength => MaxValueLength;
    }

    public record InvalidValueLengthItem
    {
        public required string Value { get; init; }

        public required int MaxLength { get; init; }
    }

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
