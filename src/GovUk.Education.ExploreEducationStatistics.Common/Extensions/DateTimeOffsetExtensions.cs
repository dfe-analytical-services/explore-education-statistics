using System;

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
            TimeSpan.Zero);
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
            TimeSpan.Zero);
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
}
