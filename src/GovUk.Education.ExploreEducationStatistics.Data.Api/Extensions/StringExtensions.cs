using System.Globalization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions
{
    public static class StringExtensions
    {
        public static string CamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var s = input.Replace("_", " ");
            s = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s).Replace(" ", string.Empty);
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }
}