using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AsMidnightLocalTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Local);
        }
    }
}