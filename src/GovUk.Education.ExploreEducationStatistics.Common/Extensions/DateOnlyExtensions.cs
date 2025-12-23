using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DateOnlyExtensions
{
    private static TimeZoneInfo UkTimeZone => TimeZoneUtils.GetUkTimeZone();

    public static DateTimeOffset GetUkStartOfDayUtc(this DateOnly date)
    {
        // Get a DateTime representing the date at 00:00:00 (midnight)
        var midnightDateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        // Treat the date as midnight in the UK time zone and convert it to UTC
        var ukMidnightDateTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(midnightDateTime, UkTimeZone);

        // Return a DateTimeOffset in UTC
        return new DateTimeOffset(ukMidnightDateTimeInUtc, TimeSpan.Zero);
    }
}
