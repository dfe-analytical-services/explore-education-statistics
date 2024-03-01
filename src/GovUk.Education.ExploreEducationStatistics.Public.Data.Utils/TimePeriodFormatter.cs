using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

public static partial class TimePeriodFormatter
{
    [GeneratedRegex(@"^[0-9]{4}([0-9]{2})?$")]
    private static partial Regex YearRegex();

    public static string Format(int year, TimeIdentifier timeIdentifier)
    {
        var match = YearRegex().Match(year.ToString());

        if (!match.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        if (match.Length == 4)
        {
            return TimePeriodLabelFormatter.Format(year, timeIdentifier);
        }

        var firstTwoDigitYear = int.Parse(match.Groups[0].Value.Substring(2, 2));
        var secondTwoDigitYear = int.Parse(match.Groups[1].Value);

        year /= 100;

        if (firstTwoDigitYear == 99 && secondTwoDigitYear == 0)
        {
            return TimePeriodLabelFormatter.Format(year, timeIdentifier);
        }

        if (secondTwoDigitYear != firstTwoDigitYear + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        return TimePeriodLabelFormatter.Format(year, timeIdentifier);
    }
}
