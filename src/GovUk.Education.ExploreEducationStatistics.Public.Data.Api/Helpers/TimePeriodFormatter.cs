using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Helpers;

public static class TimePeriodFormatter
{
    private static readonly Regex YearRegex = new(@"^[0-9]{4}([0-9]{2})?$");

    public static string Format(int year, TimeIdentifier timeIdentifier)
    {
        var match = YearRegex.Match(year.ToString());

        if (!match.Success)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (match.Length == 6)
        {
            year /= 100;
        }

        return TimePeriodLabelFormatter.Format(year, timeIdentifier);
    }
}
