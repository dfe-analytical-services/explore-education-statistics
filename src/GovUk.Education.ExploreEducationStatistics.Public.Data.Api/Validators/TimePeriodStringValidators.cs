using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class TimePeriodStringValidators
{
    public static IRuleBuilderOptionsConditions<T, string> TimePeriodString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Custom((value, context) => Validate([value], context, usePluralMessage: false));
    }

    public static IRuleBuilderOptionsConditions<T, IEnumerable<string>> OnlyTimePeriodStrings<T>(
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
                error: ValidationErrorMessages.TimePeriodFormat,
                detail: new FormatErrorDetail(invalidFormats),
                usePluralMessage: usePluralMessage
            );
            return;
        }

        var timePeriods = distinctValues
            .Select(value => new ParsedTimePeriod(value))
            .ToList();

        var invalidRanges = new List<string>();
        var invalidCodes = new List<string>();

        foreach (var timePeriod in timePeriods)
        {

            if (!HasValidRange(timePeriod))
            {
                invalidRanges.Add(timePeriod.String);
                continue;
            }

            if (!HasAllowedCode(timePeriod))
            {
                invalidCodes.Add(timePeriod.String);
            }
        }

        if (invalidRanges.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.TimePeriodRange,
                detail: new RangeErrorDetail(invalidRanges),
                usePluralMessage: usePluralMessage
            );
        }

        if (invalidCodes.Count != 0)
        {
            context.AddFailure(
                error: ValidationErrorMessages.TimePeriodAllowedCode,
                detail: new AllowedCodeErrorDetail(invalidCodes),
                usePluralMessage: usePluralMessage
            );
        }
    }

    private static readonly IReadOnlySet<TimeIdentifier> AllowedTimeIdentifiers =
        TimeIdentifierUtils.DataEnums.ToHashSet();

    [GeneratedRegex(@"^[0-9]{4}(\/[0-9]{4})?\|[A-Z0-9]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasValidRange(ParsedTimePeriod timePeriod)
    {
        // The period is not a range.
        if (!timePeriod.Period.Contains('/'))
        {
            return true;
        }

        var rangeParts = timePeriod.Period.Split('/');
        var start = rangeParts[0];
        var end = rangeParts[1];

        return int.TryParse(start, out var startYear)
               && int.TryParse(end, out var endYear)
               && startYear < endYear;
    }

    private static bool HasAllowedCode(ParsedTimePeriod timePeriod)
    {
        return EnumUtil.TryGetFromEnumValue<TimeIdentifier>(timePeriod.Code, out var code)
               && AllowedTimeIdentifiers.Contains(code);
    }

    public record FormatErrorDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid)
    {
        public string ExpectedFormat => "{period}|{code}";
    }

    public record RangeErrorDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid);

    public record AllowedCodeErrorDetail(IEnumerable<string> Invalid) : InvalidItemsErrorDetail<string>(Invalid)
    {
        public IReadOnlyList<string> Allowed => TimeIdentifierUtils.DataCodes.Order().ToList();
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
    }
}
