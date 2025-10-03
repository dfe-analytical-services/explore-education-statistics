#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.Char;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public static class NamingUtils
{
    public static string CreateReleaseSlug(int year, TimeIdentifier timePeriodCoverage, string? label = null)
    {
        var trimmedLowercaseLabel = label?.Trim().ToLower();

        return SlugFromTitle(
            $"{Format(year, timePeriodCoverage)}{(string.IsNullOrWhiteSpace(trimmedLowercaseLabel) ? "" : $"-{trimmedLowercaseLabel}")}"
        );
    }

    public static string SlugFromTitle(string title)
    {
        // NOTE: If you change anything here, you should also update `slugFromTitle.ts` in the frontend
        var replaceNonAlphaNumericWithSpace = ReplaceNonAlphaNumericWithSpaceAndTrim(title);
        var toLower = new string(replaceNonAlphaNumericWithSpace.Select(ToLower).ToArray());
        var removeMultipleSpaces = Regex.Replace(toLower, @"\s+", " ");
        var replaceSpaces = new string(removeMultipleSpaces.Select(c => IsWhiteSpace(c) ? '-' : c).ToArray());
        return replaceSpaces;
    }

    private static string ReplaceNonAlphaNumericWithSpaceAndTrim(string s)
    {
        return new string(s.Select(ReplaceNonAlphaNumericWithSpace).ToArray()).Trim();
    }

    private static Func<char, char> ReplaceNonAlphaNumericWithSpace =>
        c => IsLetter(c) || IsWhiteSpace(c) || IsDigit(c) ? c : ' ';
}
