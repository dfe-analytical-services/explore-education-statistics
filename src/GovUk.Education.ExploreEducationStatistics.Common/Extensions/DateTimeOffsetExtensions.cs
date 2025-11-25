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

    public static DateTimeOffset GetUkStartOfDayOn(this DateTimeOffset dateTime, int daysAfter = 0)
    {
        // Convert the source instant to the UK local time
        var ukLocal = dateTime.ConvertToUkTimeZone(); // returns DateTimeOffset with UK offset

        // Get the local calendar date in UK, then add days
        var targetUkLocalDate = ukLocal.Date.AddDays(daysAfter); // Date is midnight of that day (DateTime)

        // Get the correct offset for that local UK date (handles DST transitions)
        var targetUkOffset = targetUkLocalDate.GetUkOffset();

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
        var targetUkOffset = targetUkLocalDate.GetUkOffset();

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

    public static bool IsSameCalendarDay(this DateTimeOffset dateTime1, DateTimeOffset dateTime2) =>
        dateTime1.ConvertToUkTimeZone().Date == dateTime2.ConvertToUkTimeZone().Date;

    public static DateTimeOffset ConvertToUkTimeZone(this DateTimeOffset dateTime) =>
        TimeZoneInfo.ConvertTime(dateTime, DateTimeExtensions.GetUkTimeZone());
}
