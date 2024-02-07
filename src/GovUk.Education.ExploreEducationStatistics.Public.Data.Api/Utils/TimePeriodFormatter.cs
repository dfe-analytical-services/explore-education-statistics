using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;

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

        if (match.Length == 6)
        {
            year /= 100;
        }

        return TimePeriodLabelFormatter.Format(year, timeIdentifier);
    }
}
