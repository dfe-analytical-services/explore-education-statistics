using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

public static partial class TimePeriodFormatter
{
    [GeneratedRegex(
        @"^[0-9]{4}(\/[0-9]{4})?$",
        RegexOptions.Compiled,
        matchTimeoutMilliseconds: 200
    )]
    private static partial Regex PeriodRegexGenerated();

    private static readonly Regex PeriodRegex = PeriodRegexGenerated();

    /// <summary>
    /// Format a time period to its human-readable label.
    /// </summary>
    /// <param name="period">The time period to format</param>
    /// <param name="identifier">The time period identifier to use in the label</param>
    /// <returns>The time period's human-readable label</returns>
    public static string FormatLabel(string period, TimeIdentifier identifier)
    {
        var match = PeriodRegex.Match(period);

        if (!match.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        var firstYear = int.Parse(period[..4]);

        if (match.Length == 4)
        {
            return TimePeriodLabelFormatter.Format(firstYear, identifier);
        }

        var secondYear = int.Parse(period[5..9]);

        if (secondYear != firstYear + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        return TimePeriodLabelFormatter.Format(firstYear, identifier);
    }

    [GeneratedRegex(@"^[0-9]{4}([0-9]{2})?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex CsvPeriodRegexGenerated();

    private static readonly Regex CsvPeriodRegex = CsvPeriodRegexGenerated();

    /// <summary>
    /// Format a time period from a CSV (e.g. 202021) to a standard format (e.g. 2020/2021).
    /// </summary>
    /// <param name="period">The CSV time period to parse</param>
    /// <returns>The time period in standard format</returns>
    public static string FormatFromCsv(string period)
    {
        var match = CsvPeriodRegex.Match(period);

        if (!match.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        if (match.Length == 4)
        {
            return period;
        }

        var year = int.Parse(period[..4]);
        var firstTwoDigitYear = int.Parse(period[2..4]);
        var secondTwoDigitYear = int.Parse(period[4..]);

        if (firstTwoDigitYear == 99 && secondTwoDigitYear == 0)
        {
            return $"{year}/{year + 1}";
        }

        if (secondTwoDigitYear != firstTwoDigitYear + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        return $"{year}/{year + 1}";
    }

    /// <summary>
    /// Format a time period to its CSV format (e.g. 202021).
    /// </summary>
    /// <param name="period">The time period (in standard format) to format</param>
    /// <returns>The time period in CSV format</returns>
    public static string FormatToCsv(string period)
    {
        var match = PeriodRegex.Match(period);

        if (!match.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        if (match.Length == 4)
        {
            return period;
        }

        var firstYear = int.Parse(period[..4]);
        var secondYear = int.Parse(period[5..]);

        if (secondYear != firstYear + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        return $"{period[..4]}{period[7..]}";
    }
}
