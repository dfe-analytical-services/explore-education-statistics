using System;
using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

/// <summary>
/// This enum allows us to specify that, in addition to the cache duration requested when setting an item in the cache,
/// the specified cache duration will be truncated so as to not exceed certain regular points of time during the day.
/// 
/// As an example, if an item is being set in the cache at 2:27pm with a requested cache duration of 5 minutes, it would
/// expire at 2:32pm assuming that no ExpirySchedule was also requested when setting that item.
/// 
/// However, if an item is being set in the cache at 2:27pm with a requested cache duration of 5 minutes AND an
/// ExpirySchedule of HalfHourly, then the cache duration will be truncated to expire the item at 2:30pm (i.e. on the
/// half hour).
/// 
/// This allows us to request reasonably-sized cache durations for our items, but allowing us to expire them at certain
/// critical points during the day where we expect these items to potentially change.
///  
/// </summary>
public enum ExpirySchedule
{
    Hourly,
    HalfHourly,
    None
}

public record MemoryCacheConfiguration(ExpirySchedule ExpirySchedule, int CacheDurationInSeconds)
{
    public List<int> GetDailyExpiryStartTimesInSeconds()
    {
        return ExpirySchedule switch
        {
            ExpirySchedule.None => new List<int>(),
            ExpirySchedule.HalfHourly => Range(0, 48).Select(i => i * 30 * 60).ToList(),
            ExpirySchedule.Hourly => Range(0, 24).Select(i => i * 60 * 60).ToList(),
            _ => throw new ArgumentException($"Unhandled {nameof(ExpirySchedule)} value {ExpirySchedule}")
        };
    }
}