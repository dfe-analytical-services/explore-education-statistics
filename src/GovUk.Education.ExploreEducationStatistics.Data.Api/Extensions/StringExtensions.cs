using System.Globalization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions
{
    public static class StringExtensions
    {
        public static string PascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var s = input.Replace("_", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s).Replace(" ", string.Empty);
        }
    }
}