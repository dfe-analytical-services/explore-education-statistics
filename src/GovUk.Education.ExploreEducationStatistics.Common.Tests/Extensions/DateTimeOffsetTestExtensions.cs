using System;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class DateTimeOffsetTestExtensions
{
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
        Assert.NotNull(offset);
        return TruncateNanoseconds((DateTimeOffset)offset);
    }
}
