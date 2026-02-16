#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset? AdjustUkLocalMidnightTo0930(this DateTimeOffset? dateTimeOffset) =>
        dateTimeOffset?.AdjustUkLocalMidnightTo0930();

    public static DateTimeOffset AdjustUkLocalMidnightTo0930(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.IsUkLocalMidnight() ? dateTimeOffset.AddHours(9).AddMinutes(30) : dateTimeOffset;

    public static bool IsUkLocalMidnight(this DateTimeOffset? dateTimeOffset) =>
        dateTimeOffset.HasValue && dateTimeOffset.Value.IsUkLocalMidnight();

    public static bool IsUkLocalMidnight(this DateTimeOffset dateTimeOffset)
    {
        // Convert the DateTimeOffset to UK local time
        var ukLocalDateTimeOffset = TimeZoneInfo.ConvertTime(dateTimeOffset, TimeZoneUtils.GetUkTimeZone());

        // Return true if the time element is exactly 00:00:00
        return ukLocalDateTimeOffset.TimeOfDay == TimeSpan.Zero;
    }
}
