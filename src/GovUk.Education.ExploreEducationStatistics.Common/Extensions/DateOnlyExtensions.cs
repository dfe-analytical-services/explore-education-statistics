using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DateOnlyExtensions
{
    public static DateTimeOffset GetUkStartOfDayUtc(this DateOnly date)
    {
        // Get a DateTime representing the date at midnight
        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified); // Date is 00:00:00 (midnight) of that day

        var ukTimeZone = TimeZoneUtils.GetUkTimeZone();

        // Get the offset from UTC for the date at midnight in the UK, taking into account daylight saving time
        var ukOffset = ukTimeZone.GetUtcOffset(startOfDay);

        // Create a DateTimeOffset with the correct offset from UTC for the date
        var ukStartOfDay = new DateTimeOffset(startOfDay, ukOffset);

        // Convert the DateTimeOffset to UTC
        return ukStartOfDay.ToUniversalTime();
    }
}
