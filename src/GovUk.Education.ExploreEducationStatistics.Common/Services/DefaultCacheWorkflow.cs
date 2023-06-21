#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public record DefaultCacheWorkflow<TCacheKey>(
        Func<TCacheKey, Task<object?>> GetCachedItemFn,
        Func<TCacheKey, object, Task> CacheItemFn) {
    
    public async Task<object> GetOrCreateItemAsync(object cacheKey, Func<Task<object>> createItemFn)
    {
        if (cacheKey is not TCacheKey typedCacheKey)
        {
            throw new ArgumentException($"Cache Key should be of type {typeof(TCacheKey)} but was {cacheKey.GetType()}");
        }
        
        var cachedItem = await GetCachedItemFn.Invoke(typedCacheKey);
        
        
    }
}