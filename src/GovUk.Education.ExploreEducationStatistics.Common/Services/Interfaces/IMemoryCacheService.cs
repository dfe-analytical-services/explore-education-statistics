#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

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
public interface IMemoryCacheService
{
    Task<object?> GetItem(IMemoryCacheKey cacheKey, Type targetType);

    Task SetItem<TItem>(
        IMemoryCacheKey cacheKey, 
        TItem item, 
        MemoryCacheConfiguration configuration,
        DateTime? nowUtc = null);
}