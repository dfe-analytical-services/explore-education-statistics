using System;
using System.Runtime.InteropServices;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DateTimeExtensions
    {
        /**
         * The resulting value corresponds to this DateTime with the time-of-day part set to zero GMT/BST as UTC.
         */
        public static DateTime AsStartOfDayUtc(this DateTime dateTime)
        {
            var dateTimeAtMidnight = dateTime.Date;
            return TimeZoneInfo.ConvertTimeToUtc(dateTimeAtMidnight, GetGmtStandardTimeTimezone());
        }

        private static TimeZoneInfo GetGmtStandardTimeTimezone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/London");
        }
    }
}