using System;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Char;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public static class NamingUtils
    {
        public static string SlugFromTitle(string title)
        {
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
}