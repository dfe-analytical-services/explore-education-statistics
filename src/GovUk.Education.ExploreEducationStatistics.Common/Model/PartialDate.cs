#nullable enable
using System;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static System.Int32;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public record PartialDate
    {
        public static readonly Regex YearRegex = new(@"^([0-9]{4})?$");
        public static readonly Regex MonthRegex = new(@"^([0-9]{1,2})?$");
        public static readonly Regex DayRegex = new(@"^([0-9]{1,2})?$");

        private readonly string? _month;
        private readonly string? _day;

        public string Year { get; init; } = Empty;

        public string Month
        {
            get => _month ?? Empty;
            init => _month = value;
        }

        public string Day
        {
            get => _day ?? Empty;
            init => _day = value;
        }

        public bool IsValid()
        {
            var hasYear = !Year.IsNullOrWhitespace();
            var hasMonth = !Month.IsNullOrWhitespace();
            var hasDay = !Day.IsNullOrWhitespace();

            if (!hasYear)
            {
                return false;
            }

            if (hasDay && !hasMonth)
            {
                return false;
            }

            if (!YearRegex.Match(Year).Success || !MonthRegex.Match(Month).Success || !DayRegex.Match(Day).Success)
            {
                return false; // Failed rudimentary number validation
            }

            if (!EmptyOrBetween(Month, 1, 12) || !EmptyOrBetween(Day, 1, 31))
            {
                return false; // Failed more precise number validation
            }

            if (hasMonth && hasDay)
            {
                // We at least have a month and a day so at the very least we can check that they are acceptable
                // together. If we have a year we can do even more if we do not then we use a leap year as this gives a
                // wider range of acceptable values.
                const int leapYear = 2016;
                var yearToCheckAgainst = !IsNullOrEmpty(Year) ? Parse(Year) : leapYear;
                var intDay = Parse(Day);
                var intMonth = Parse(Month);
                var daysInMonth = DateTime.DaysInMonth(yearToCheckAgainst, intMonth);
                if (intDay > daysInMonth || intDay < 1)
                {
                    return false; // Failed more specific day / month validation
                }
            }

            return true;
        }

        public bool IsEmpty()
        {
            var hasYear = !Year.IsNullOrWhitespace();
            var hasMonth = !Month.IsNullOrWhitespace();
            var hasDay = !Day.IsNullOrWhitespace();

            return !hasYear && !hasMonth && !hasDay;
        }

        private static bool EmptyOrBetween(string value, int lower, int upper)
        {
            return IsNullOrEmpty(value) || (Parse(value) >= lower && Parse(value) <= upper);
        }
    }
}