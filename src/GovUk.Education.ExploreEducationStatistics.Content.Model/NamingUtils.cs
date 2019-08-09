using System;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.Char;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public static class NamingUtils
    {
        private static readonly Regex YearRegex = new Regex(@"^([0-9]{4})?$");

        public static string SlugFromTitle(string title)
        {
            var removeNonAlphaNumeric =
                new string(title.Where(c => IsLetter(c) || IsWhiteSpace(c) || IsDigit(c)).ToArray()).Trim();
            var toLower = new string(removeNonAlphaNumeric.Select(ToLower).ToArray());
            var removeMultipleSpaces = Regex.Replace(toLower, @"\s+", " ");
            var replaceSpaces = new string(removeMultipleSpaces.Select(c => IsWhiteSpace(c) ? '-' : c).ToArray());
            return replaceSpaces;
        }

        public static string ReleaseYearTitle(string year, TimeIdentifier coverage)
        {
            // Calendar year time identifiers we just use the year, all others we use a year range. We express this range in the format e.g. 2019/20
            if (!IsNullOrEmpty(year) && YearRegex.Match(year).Success &&
                !TimeIdentifierCategory.CalendarYear.GetTimeIdentifiers().Contains(coverage))
            {
                var releaseStartYear = Int32.Parse(year);
                var releaseEndYear = (releaseStartYear % 100) + 1; // Only want the last two digits
                return releaseStartYear + "/" + releaseEndYear;
            }

            // For calendar year time identifiers we just want the year not a range. If there is no year then we just output the time period identifier
            return IsNullOrEmpty(year) ? "" : year;
        }

        public static string ReleaseCoverageTitle(TimeIdentifier coverage) => coverage.GetEnumLabel();

        public static string ReleaseTitle(string year, TimeIdentifier coverage)
        {
            var yearTitle = ReleaseYearTitle(year, coverage);
            return ReleaseCoverageTitle(coverage) + (IsNullOrEmpty(yearTitle) ? "" : " " + yearTitle);
        }
    }
}