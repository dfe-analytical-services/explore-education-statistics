#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static System.Linq.Enumerable;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

/// <summary>
/// Interface for services that handle in-memory caches.
/// 
/// There is deliberately no `Delete` method defined on this interface, as the client of this service should instead be
/// setting rules around cache expiry of the items being cached.
/// 
/// The most important reason that there is no `Delete` method for use is that this in-memory caching strategy is
/// not distributed in any way and so a Delete operation invoked on a cache on a particular server instance would have
/// no effect on any additional servers running the same service elsewhere e.g. in a horizontally scaled service.
/// 
/// Items that are subject to regular updates are therefore advised to be cached for a very short-lived timeframe so as
/// to prevent consumers from having to wait too long before the updated content is available.  
/// 
/// </summary>
public interface IInMemoryCacheService
{
    Task<object?> GetItem(IInMemoryCacheKey cacheKey, Type targetType);

    Task SetItem<TItem>(
        IInMemoryCacheKey cacheKey, 
        TItem item, 
        InMemoryCacheConfiguration configuration,
        DateTime? nowUtc = null);
}


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

public record InMemoryCacheConfiguration(ExpirySchedule ExpirySchedule, int CacheDurationInSeconds)
{
    public List<int> GetDailyExpiryStartTimesInSeconds()
    {
        return ExpirySchedule switch
        {
            ExpirySchedule.None => new List<int>(),
            ExpirySchedule.HalfHourly => Range(0, 48).Select(i => i3060).ToList(),
            ExpirySchedule.Hourly => Range(0, 24).Select(i => i6060).ToList(),
            _ => throw new ArgumentException($"Unhandled {nameof(ExpirySchedule)} value {ExpirySchedule}")
        };
    }
        

}