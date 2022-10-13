#nullable enable
using System;
using System.Runtime.InteropServices;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DateTimeExtensions
    {
        /**
         * The resulting UTC DateTime will correspond to the start of the day
         * in the specified time zone (defaults to UK).
         *
         * For example, 2020-06-06T16:00:00 converts to:
         * - 2020-06-05T23:00:00 (when BST)
         * - 2020-06-06T00:00:00 (when GMT)
         */
        public static DateTime AsStartOfDayUtcForTimeZone(this DateTime dateTime, TimeZoneInfo? timezone = null)
        {
            var dateTimeStartOfDay = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(dateTimeStartOfDay, timezone ?? GetUkTimeZone());
        }

        public static DateTime ConvertUtcToUkTimeZone(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, GetUkTimeZone());
        }

        public static TimeZoneInfo GetUkTimeZone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/London");
        }
    }
}
