#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class DateTimeProvider
{
    private readonly DateTime? _fixedDateTimeUtc;

    /// <summary>
    /// Allows a fixed DateTime to be provided for testing.
    /// </summary>
    /// <param name="fixedDateTimeUtc"></param>
    public DateTimeProvider(DateTime? fixedDateTimeUtc = null)
    {
        _fixedDateTimeUtc = fixedDateTimeUtc;
    }

    public DateTime UtcNow => _fixedDateTimeUtc ?? DateTime.UtcNow;
}
