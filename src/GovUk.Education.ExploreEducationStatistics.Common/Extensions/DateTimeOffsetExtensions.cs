using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Strip the microsecond and nanosecond components from a DateTimeOffset. Helpful for testing in scenarios where
    /// precision can change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from JSON, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateMicroseconds(this DateTimeOffset offset)
    {
        return new DateTimeOffset(
            offset.Year,
            offset.Month,
            offset.Day,
            offset.Hour,
            offset.Minute,
            offset.Second,
            offset.Millisecond,
            0,
            TimeSpan.Zero
        );
    }

    /// <summary>
    /// Strip the microsecond and nanosecond components from a DateTimeOffset. Helpful for testing in scenarios where
    /// precision can change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from JSON, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateMicroseconds(this DateTimeOffset? offset)
    {
        if (offset == null)
        {
            throw new ArgumentException("offset cannot be null when truncating microseconds");
        }

        return TruncateMicroseconds(offset.Value);
    }

    /// <summary>
    /// Strip the nanosecond component from a DateTimeOffset. Helpful for testing in scenarios where precision can
    /// change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from PostgreSQL, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateNanoseconds(this DateTimeOffset offset)
    {
        return new DateTimeOffset(
            offset.Year,
            offset.Month,
            offset.Day,
            offset.Hour,
            offset.Minute,
            offset.Second,
            offset.Millisecond,
            offset.Microsecond,
            TimeSpan.Zero
        );
    }

    /// <summary>
    /// Strip the nanosecond component from a DateTimeOffset. Helpful for testing in scenarios where precision can
    /// change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from PostgreSQL, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateNanoseconds(this DateTimeOffset? offset)
    {
        if (offset == null)
        {
            throw new ArgumentException("offset cannot be null when truncating nanoseconds");
        }

        return TruncateNanoseconds(offset.Value);
    }

    [Pure]
    private static DateTimeOffset AsXOfDayForTimeZone(
        this DateTimeOffset instant,
        DayPeriod period,
        TimeZoneInfo timeZone = null
    )
    {
        timeZone ??= DateTimeExtensions.GetUkTimeZone();
        var local = TimeZoneInfo.ConvertTime(instant, timeZone);

        var localMidnightDateTime =
            period == DayPeriod.LastTick
                ? new DateTime(local.Year, local.Month, local.Day, 23, 59, 59, DateTimeKind.Unspecified)
            : period == DayPeriod.Midnight
                ? new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Unspecified)
            : throw new ArgumentException("Invalid day period");
        var offset = timeZone.GetUtcOffset(localMidnightDateTime);
        var lastTickOrMidnightOfDayLocal = new DateTimeOffset(localMidnightDateTime, offset);

        return lastTickOrMidnightOfDayLocal;
    }

    [Pure]
    public static DateTimeOffset AsStartOfDayForUkTimeZone(this DateTimeOffset instant, TimeZoneInfo timeZone = null) =>
        AsXOfDayForTimeZone(instant, DayPeriod.Midnight, timeZone);

    [Pure]
    public static DateTimeOffset AsEndOfDayForUkTimeZone(this DateTimeOffset instant, TimeZoneInfo timeZone = null) =>
        AsXOfDayForTimeZone(instant, DayPeriod.LastTick, timeZone);

    public static bool IsSameLocalDay(this DateTimeOffset date1, DateTimeOffset date2)
    {
        var ukDate1 = date1.ConvertToUkTimeZone();
        var ukDate2 = date2.ConvertToUkTimeZone();
        return ukDate1.Date == ukDate2.Date;
    }

    [Pure]
    public static DateTimeOffset ConvertToUkTimeZone(this DateTimeOffset dateTime) =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            dateTime,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/London"
        );
}

public enum DayPeriod
{
    Midnight,
    LastTick,
}
