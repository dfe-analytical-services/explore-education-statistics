#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset? AdjustUkLocalMidnightTo0930(this DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset == null)
        {
            return null;
        }
        return IsUkLocalMidnight(dateTimeOffset.Value)
            ? dateTimeOffset.Value.AddHours(9).AddMinutes(30)
            : dateTimeOffset;
    }

    private static bool IsUkLocalMidnight(DateTimeOffset dateTimeOffset)
    {
        // Convert the DateTimeOffset to UK local time
        var ukLocalDateTimeOffset = TimeZoneInfo.ConvertTime(dateTimeOffset, TimeZoneUtils.GetUkTimeZone());

        // Return true if the time element is exactly 00:00:00
        var timeOfDay = ukLocalDateTimeOffset.TimeOfDay;
        return timeOfDay == TimeSpan.Zero;
    }
}
