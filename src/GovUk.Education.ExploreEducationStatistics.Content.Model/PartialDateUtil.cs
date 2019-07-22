using System;
using System.Globalization;
using static System.Int32;
using static System.String;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public static class PartialDateUtil
    {
        public static bool PartialDateValid(string partialDate)
        {
            var reg = new Regex(@"([0-9]{4})?-([0-1][0-9])?-([0-3][0-9])?");
            return reg.Match(partialDate).Groups.Count == 3;
        }


        public static PartialDate PartialDateValid2(string partialDate)
        {
            var reg = new Regex(@"([0-9]{4})?-([0-1][0-9])?-([0-3][0-9])?");
            var matcher = reg.Match(partialDate);

            if (!matcher.Success)
            {
                return null; // Fail
            }
            
            var year = reg.Match(partialDate).Groups[1].Value;
            var month = reg.Match(partialDate).Groups[2].Value;
            var day = reg.Match(partialDate).Groups[3].Value;
            
            if (!emptyOrBetween(month, 1, 12) || emptyOrBetween(day, 1, 31))
            {
                return null; // Fail
            }

            if (IsNullOrEmpty(year) && !IsNullOrEmpty(month) && !IsNullOrEmpty(day))
            {
                // Do some more validation
            }

            if (!IsNullOrEmpty(year) && !IsNullOrEmpty(month) && !IsNullOrEmpty(day))
            {
                // Do some more validation - this is really only for leap years
                DateTime date;
                if (!DateTime.TryParseExact(partialDate, "YYYY-MM-DD", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime)){
                    return null; // Fail
                }
            }
            
            return new PartialDate {Year = year, Month = month, Day = day};
        }

        private static bool emptyOrBetween(string value, int lower, int upper)
        {
            return IsNullOrEmpty(value) && Parse(value) >= lower && Parse(value) <= upper;
        }

        public class PartialDate
        {
            public string Year { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
        }

//        public static string Year(string partialDate)
//        {
//            
//        }
    }
}