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

    public static DateTimeOffset GetUkEndOfDayUtc(this DateOnly date, bool includeFractionalSeconds = true)
    {
        // Get a TimeOnly value set to the end of the day.
        // This is either 23:59:59.9999999 or 23:59:59 depending on whether fractional seconds should be included.
        var timeAtEndOfDay = includeFractionalSeconds ? TimeOnly.MaxValue : new TimeOnly(23, 59, 59);

        // Get a DateTime set to the date of this DateOnly instance and the time at the end of the day
        var endOfDayDateTime = date.ToDateTime(timeAtEndOfDay, DateTimeKind.Unspecified);

        // Treat this as being in the UK time zone and convert it to UTC
        var ukEndOfDayDateTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDayDateTime, UkTimeZone);

        // Return a DateTimeOffset in UTC
        return new DateTimeOffset(ukEndOfDayDateTimeInUtc, TimeSpan.Zero);
    }
}
