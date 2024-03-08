using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class TimePeriodStringValidators
{
    private const string ExpectedFormat = "{period}|{code}";

    public static IRuleBuilderOptionsConditions<T, string> TimePeriodString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Custom((value, context) =>
        {
            if (!HasValidFormat(value))
            {
                context.AddFailure(
                    message: ValidationMessages.TimePeriodFormat,
                    detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                );
                return;
            }

            var timePeriod = new ParsedTimePeriod(value);

            if (!HasValidYearRange(timePeriod))
            {
                context.AddFailure(
                    message: ValidationMessages.TimePeriodYearRange,
                    detail: new RangeErrorDetail(timePeriod.String)
                );
            }

            if (!HasAllowedCode(timePeriod))
            {
                context.AddFailure(
                    message: ValidationMessages.TimePeriodAllowedCode,
                    detail: new AllowedCodeErrorDetail(timePeriod.String)
                    {
                        Allowed = timePeriod.HasRangePeriod() ? AllowedRangeCodes : AllowedCodes
                    }
                );
            }
        });
    }

    private static readonly IReadOnlySet<TimeIdentifier> AllowedTimeIdentifiers =
        TimeIdentifierUtils.DataEnums.ToHashSet();

    private static readonly IReadOnlySet<TimeIdentifier> AllowedRangeTimeIdentifiers =
        AllowedTimeIdentifiers
            .Where(
                identifier => identifier.GetEnumAttribute<TimeIdentifierMetaAttribute>().YearFormat
                    is TimePeriodYearFormat.Academic or TimePeriodYearFormat.Fiscal
            )
            .ToHashSet();

    private static readonly IReadOnlyList<string> AllowedCodes =
        TimeIdentifierUtils.DataCodes
            .Order()
            .ToList();

    private static readonly IReadOnlyList<string> AllowedRangeCodes =
        AllowedRangeTimeIdentifiers
            .Select(identifier => identifier.GetEnumValue())
            .Order()
            .ToList();

    [GeneratedRegex(@"^[0-9]{4}(\/[0-9]{4})?\|[A-Z0-9]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasValidYearRange(ParsedTimePeriod timePeriod)
    {
        // The period is not a range.
        if (!timePeriod.HasRangePeriod())
        {
            return true;
        }

        var rangeParts = timePeriod.Period.Split('/');
        var start = rangeParts[0];
        var end = rangeParts[1];

        return int.TryParse(start, out var startYear)
               && int.TryParse(end, out var endYear)
               && endYear == startYear + 1;
    }

    private static bool HasAllowedCode(ParsedTimePeriod timePeriod)
    {
        if (!EnumUtil.TryGetFromEnumValue<TimeIdentifier>(timePeriod.Code, out var code))
        {
            return false;
        }

        return timePeriod.HasRangePeriod()
            ? AllowedRangeTimeIdentifiers.Contains(code)
            : AllowedTimeIdentifiers.Contains(code);
    }

    public record RangeErrorDetail(string Value) : InvalidErrorDetail<string>(Value);

    public record AllowedCodeErrorDetail(string Value) : InvalidErrorDetail<string>(Value)
    {
        public required IReadOnlyList<string> Allowed { get; init; }
    }

    public record ParsedTimePeriod
    {
        public string String { get; init; }
        
        public string Period { get; init; }
        
        public string Code { get; init; }

        public ParsedTimePeriod(string value)
        {
            var parts = value.Split('|');

            String = value;
            Period = parts[0];
            Code = parts[1];
        }

        public bool HasRangePeriod() => Period.Contains('/');
    }
}
