using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    private static TimeZoneInfo UkTimeZone => TimeZoneUtils.GetUkTimeZone();

    /// <summary>
    /// <para>
    /// Adjusts the provided <paramref name="dateTimeOffset"/> to the UK time zone and returns a
    /// <see cref="DateTimeOffset"/> representing the start of that UK day, accounting for daylight saving time.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item>
    /// If the DateTimeOffset is "2025-06-01T12:00:00 +00:00",
    /// after converting to the UK time zone (BST, UTC+1), "2025-06-01T00:00:00 +01:00" would be the start of the day,
    /// which corresponds to "2025-05-31T23:00:00 +00:00" in UTC which is returned.
    /// </item>
    /// <item>
    /// If the DateTimeOffset is "2025-01-01T12:00:00 +00:00",
    /// after converting to the UK time zone (GMT, UTC+0), "2025-01-01T00:00:00 +00:00" would be the start of the day,
    /// which corresponds to "2025-01-01T00:00:00 +00:00" in UTC which is returned.
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="dateTimeOffset">The input <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A <see cref="DateTimeOffset"/> in UTC corresponding to the start of the UK day for the provided input.</returns>
    public static DateTimeOffset GetUkStartOfDayUtc(this DateTimeOffset dateTimeOffset)
    {
        // Convert the date and time to the UK time zone
        var inUkZone = dateTimeOffset.ConvertToUkTimeZone();

        // Get the date component of that date in the UK time zone (DateTime at midnight)
        var ukMidnightDateTime = inUkZone.Date;

        // Get the correct UTC offset for midnight on that date in the UK time zone
        var ukZoneOffset = UkTimeZone.GetUtcOffset(ukMidnightDateTime);

        // Create a DateTimeOffset with the correct UTC offset for midnight on that date in the UK time zone
        var ukMidnightDateTimeOffset = new DateTimeOffset(ukMidnightDateTime, ukZoneOffset);

        // Convert the DateTimeOffset to UTC
        return ukMidnightDateTimeOffset.ToUniversalTime();
    }

    /// <summary>
    /// <para>
    /// Adjusts the provided <paramref name="dateTimeOffset"/> to the UK time zone and returns a <see cref="DateOnly"/>.
    /// </para>
    /// <para>
    /// For example, if the DateTimeOffset is "2025-05-31T23:00:00 +00:00",
    /// after converting to the UK time zone (BST, UTC+1), "2025-06-01" would be returned as the DateOnly.
    /// </para>
    /// </summary>
    /// <param name="dateTimeOffset">The input <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A <see cref="DateOnly"/> representing the date in the UK time zone for the provided input.</returns>
    public static DateOnly ToUkDateOnly(this DateTimeOffset dateTimeOffset) => dateTimeOffset.ToDateOnly(UkTimeZone);

    private static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset, TimeZoneInfo zone)
    {
        var inTargetZone = TimeZoneInfo.ConvertTime(dateTimeOffset, zone);
        var (date, _, _) = inTargetZone;
        return date;
    }

    /// <summary>
    /// Strip the microsecond and nanosecond components from a DateTimeOffset. Helpful for testing in scenarios where
    /// precision can change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from JSON, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateMicroseconds(this DateTimeOffset dateTimeOffset)
    {
        return new DateTimeOffset(
            dateTimeOffset.Year,
            dateTimeOffset.Month,
            dateTimeOffset.Day,
            dateTimeOffset.Hour,
            dateTimeOffset.Minute,
            dateTimeOffset.Second,
            dateTimeOffset.Millisecond,
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
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateMicroseconds(this DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset == null)
        {
            throw new ArgumentException("offset cannot be null when truncating microseconds");
        }

        return TruncateMicroseconds(dateTimeOffset.Value);
    }

    /// <summary>
    /// Strip the nanosecond component from a DateTimeOffset. Helpful for testing in scenarios where precision can
    /// change throughout a DateTimeOffset's lifecycle.
    ///
    /// An example would be when comparing DateTimeOffsets when one has been retrieved from PostgreSQL, where the
    /// precision is less than that of C# itself.
    ///
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateNanoseconds(this DateTimeOffset dateTimeOffset)
    {
        return new DateTimeOffset(
            dateTimeOffset.Year,
            dateTimeOffset.Month,
            dateTimeOffset.Day,
            dateTimeOffset.Hour,
            dateTimeOffset.Minute,
            dateTimeOffset.Second,
            dateTimeOffset.Millisecond,
            dateTimeOffset.Microsecond,
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
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTimeOffset TruncateNanoseconds(this DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset == null)
        {
            throw new ArgumentException("offset cannot be null when truncating nanoseconds");
        }

        return TruncateNanoseconds(dateTimeOffset.Value);
    }

    public static DateTimeOffset GetUkStartOfDayOn(this DateTimeOffset dateTimeOffset, int daysAfter = 0)
    {
        // Convert the source instant to the UK local time
        var ukLocal = dateTimeOffset.ConvertToUkTimeZone(); // returns DateTimeOffset with UK offset

        // Get the local calendar date in UK, then add days
        var targetUkLocalDate = ukLocal.Date.AddDays(daysAfter); // Date is midnight of that day (DateTime)

        // Get the correct offset for that local UK date (handles DST transitions)
        var targetUkOffset = UkTimeZone.GetUtcOffset(targetUkLocalDate);

        // Return DateTimeOffset with the correct offset for UK at that local midnight
        return new DateTimeOffset(targetUkLocalDate, targetUkOffset);
    }

    public static DateTimeOffset GetUkEndOfDayOn(this DateTimeOffset dateTimeOffset, int daysAfter = 0)
    {
        // Convert the source instant to the UK local time
        var ukLocal = dateTimeOffset.ConvertToUkTimeZone(); // returns DateTimeOffset with UK offset

        // Get the local calendar date in UK, then add days
        var targetUkLocalDate = ukLocal.Date.AddDays(daysAfter); // Date is midnight of that day (DateTime)

        // Get the correct offset for that local UK date (handles DST transitions)
        var targetUkOffset = UkTimeZone.GetUtcOffset(targetUkLocalDate);

        // Return DateTimeOffset with the correct offset for UK at that local end-of-day
        return new DateTimeOffset(
            targetUkLocalDate.Year,
            targetUkLocalDate.Month,
            targetUkLocalDate.Day,
            23,
            59,
            59,
            targetUkOffset
        );
    }

    public static bool IsSameUkDay(this DateTimeOffset dateTimeOffset, DateTimeOffset otherDateTimeOffset) =>
        dateTimeOffset.ConvertToUkTimeZone().Date == otherDateTimeOffset.ConvertToUkTimeZone().Date;

    public static DateTimeOffset ConvertToUkTimeZone(this DateTimeOffset dateTimeOffset) =>
        TimeZoneInfo.ConvertTime(dateTimeOffset, UkTimeZone);
}
