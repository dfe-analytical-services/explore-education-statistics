using System.Globalization;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class StringExtensions
    {
        public static string CamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var s = PascalCase(input);
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        public static string PascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var cultInfo = CultureInfo.CurrentCulture.TextInfo;
            input = input.Replace("_", " ");
            input = Regex.Replace(input, "([A-Z]+)", " $1");
            input = cultInfo.ToTitleCase(input);
            input = input.Replace(" ", "");
            return input;
        }

        public static string SnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            input = Regex.Replace(input, "(^_([A-Z]))", "$2_$3");

            return input.TrimStart('_').ToLower();
        }

        public static string ScreamingSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return SnakeCase(input).ToUpper();
        }
    }
}