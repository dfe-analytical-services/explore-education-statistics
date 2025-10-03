using System.Globalization;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A time period to filter results by.
///
/// - `period` is the time period or range (e.g. `2020` or `2020/2021`)
/// - `code` is the code identifying the time period type (e.g. `AY`, `CY`, `M1`, `W20`)
///
/// The `period` should be a single year like `2020`, or a range like `2020/2021`.
/// Currently, only years (or year ranges) are supported.
///
/// Some time period types span two years e.g. financial year part 2 (`P2`), or may fall in a
/// latter year e.g. academic year summer term (`T3`). For these types, a singular year `period`
/// like `2020` is considered as `2020/2021`.
///
/// For example, a `period` value of `2020` is applicable to the following time periods:
///
/// - 2020 calendar year
/// - 2020/2021 academic year
/// - 2020/2021 financial year part 2 (October to March)
/// - 2020/2021 academic year's summer term
///
/// If you wish to be more explicit, you may use a range for the `period` e.g. `2020/2021`.
/// However, a range cannot be used with time period types which only span a single year,
/// for example, `2020/21` cannot be used with `CY`, `M` or `W` codes.
///
/// Some examples:
///
/// - `2020|AY` is the 2020/21 academic year
/// - `2021|FY` is the 2021/22 financial year
/// - `2020|T3` is the 2020/21 academic year's summer term
/// - `2020|P2` is the 2020/21 financial year part 2 (October to March)
/// - `2020|CY` is the 2020 calendar year
/// - `2020|W32` is 2020 week 32
/// - `2020/2021|AY` is the 2020/21 academic year
/// - `2021/2022|FY` is the 2021/22 financial year
/// </summary>
public record DataSetQueryTimePeriod
{
    /// <summary>
    /// The time period to match results by.
    ///
    /// This should be a single year like `2020` or a range like `2020/2021`.
    /// </summary>
    /// <example>2020/2021</example>
    public required string Period { get; init; }

    /// <summary>
    /// The code identifying the time period type to match results by.
    ///
    /// This should be a valid time period code e.g. `AY`, `CY`, `M1`, `W20`.
    /// </summary>
    /// <example>AY</example>
    [SwaggerEnum(typeof(TimeIdentifier))]
    public required string Code { get; init; }

    public string ParsedPeriod() => GetParsedPeriod(Period, ParsedCode());

    public TimeIdentifier ParsedCode() => EnumUtil.GetFromEnumValue<TimeIdentifier>(Code);

    public bool HasRangePeriod() => Period.Contains('/');

    public string ToTimePeriodString()
    {
        return $"{Period}|{Code}";
    }

    public static DataSetQueryTimePeriod Parse(string timePeriod)
    {
        var parts = timePeriod.Split('|');
        var period = parts[0];
        var code = parts[1];

        return new DataSetQueryTimePeriod { Period = period, Code = code };
    }

    private static string GetParsedPeriod(string period, TimeIdentifier identifier)
    {
        var metaAttribute = identifier.GetEnumAttribute<TimeIdentifierMetaAttribute>();

        if (metaAttribute.YearFormat == TimePeriodYearFormat.Default)
        {
            return !period.Contains('/')
                ? period
                : throw new ArgumentOutOfRangeException(
                    paramName: nameof(period),
                    message: "Period should be a single year and not a range."
                );
        }

        if (period.Contains('/'))
        {
            return period;
        }

        return int.TryParse(period, out var year)
            ? $"{year}/{year + 1}"
            : throw new ArgumentOutOfRangeException(
                paramName: nameof(period),
                message: "Period should be a numeric year"
            );
    }

    private static bool TryParseYear(string year, out int parsedYear) =>
        int.TryParse(year, style: NumberStyles.None, provider: null, result: out parsedYear);

    private static bool IsValidPeriodYear(string period) => TryParseYear(period, out _);

    public class Validator : AbstractValidator<DataSetQueryTimePeriod>
    {
        public Validator()
        {
            RuleFor(tp => tp.Code)
                .Must((tp, _) => IsAllowedCode(tp))
                .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code)
                .WithMessage(ValidationMessages.TimePeriodAllowedCode.Message)
                .WithState(tp => new TimePeriodAllowedCodeErrorDetail(
                    Value: tp.Code,
                    AllowedCodes: tp.HasRangePeriod() ? AllowedRangeCodes : AllowedCodes
                ))
                .DependentRules(() =>
                {
                    RuleFor(tp => tp.Period)
                        .Must(IsValidPeriodYear)
                        .WithErrorCode(ValidationMessages.TimePeriodInvalidYear.Code)
                        .WithMessage(ValidationMessages.TimePeriodInvalidYear.Message)
                        .WithState(tp => new InvalidErrorDetail<string>(tp.Period))
                        .When(tp => !tp.HasRangePeriod());

                    RuleFor(tp => tp.Period)
                        .Must(IsValidPeriodYearRange)
                        .WithErrorCode(ValidationMessages.TimePeriodInvalidYearRange.Code)
                        .WithMessage(ValidationMessages.TimePeriodInvalidYearRange.Message)
                        .WithState(tp => new InvalidErrorDetail<string>(tp.Period))
                        .When(tp => tp.HasRangePeriod());
                });
        }

        private static readonly HashSet<TimeIdentifier> AllowedTimeIdentifiers = TimeIdentifierUtils.Enums.ToHashSet();

        private static readonly HashSet<TimeIdentifier> AllowedRangeTimeIdentifiers = AllowedTimeIdentifiers
            .Where(identifier =>
                identifier.GetEnumAttribute<TimeIdentifierMetaAttribute>().YearFormat
                    is TimePeriodYearFormat.Academic
                        or TimePeriodYearFormat.Fiscal
            )
            .ToHashSet();

        private static readonly IReadOnlyList<string> AllowedCodes = TimeIdentifierUtils.Codes.Order().ToList();

        private static readonly IReadOnlyList<string> AllowedRangeCodes = AllowedRangeTimeIdentifiers
            .Select(identifier => identifier.GetEnumValue())
            .Order()
            .ToList();

        private static bool IsValidPeriodYearRange(string period)
        {
            var rangeParts = period.Split('/');
            var start = rangeParts[0];
            var end = rangeParts[1];

            return TryParseYear(start, out var startYear)
                && TryParseYear(end, out var endYear)
                && endYear == startYear + 1;
        }

        private static bool IsAllowedCode(DataSetQueryTimePeriod timePeriod)
        {
            if (!EnumUtil.TryGetFromEnumValue<TimeIdentifier>(timePeriod.Code, out var code))
            {
                return false;
            }

            return timePeriod.HasRangePeriod()
                ? AllowedRangeTimeIdentifiers.Contains(code)
                : AllowedTimeIdentifiers.Contains(code);
        }
    }
}
