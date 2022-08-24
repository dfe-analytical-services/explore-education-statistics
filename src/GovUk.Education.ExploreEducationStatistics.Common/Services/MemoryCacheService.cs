#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;
    
public class MemoryCacheService : IMemoryCacheService
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto
    };
    
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    
    public MemoryCacheService(
        IMemoryCache cache,
        ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<object?> GetItem(IMemoryCacheKey cacheKey, Type targetType)
    {
        object? cachedItem;

        try
        {
            cachedItem = _cache.Get<object?>(cacheKey);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e, 
                "Error whilst retrieving cached item for key {CacheKey} - returning null", 
                GetCacheKeyDescription(cacheKey));
            return Task.FromResult((object?) null);
        }

        if (cachedItem == null)
        {
            _logger.LogInformation(
                "Cache miss for cache key {CacheKeyDescription}", 
                GetCacheKeyDescription(cacheKey));
            return Task.FromResult((object?) null);
        }
        
        if (!targetType.IsInstanceOfType(cachedItem))
        {
            _logger.LogError(
                "Cached type {CachedItemType} is not an instance of " +
                "{TargetType} for cache key {CacheKey} - returning null",
                cachedItem.GetType(), 
                nameof(targetType), 
                GetCacheKeyDescription(cacheKey));

            return Task.FromResult((object?) null);
        }

        _logger.LogInformation(
            "Returning cached result for cache key {CacheKeyDescription}",
            GetCacheKeyDescription(cacheKey));

        return Task.FromResult(cachedItem)!;
    }

    public Task SetItem<TItem>(
        IMemoryCacheKey cacheKey,
        TItem item,
        MemoryCacheConfiguration configuration,
        DateTime? nowUtc = null)
    {
        try
        {
            var now = nowUtc ?? DateTime.UtcNow;

            DateTime absoluteExpiryTime;
            var targetAbsoluteExpiryDateTime = now.AddSeconds(configuration.DurationInSeconds);

            if (configuration.ExpirySchedule == null)
            {
                absoluteExpiryTime = targetAbsoluteExpiryDateTime;
            }
            else
            {
                var nextExpiryTime = configuration.ExpirySchedule.GetNextOccurrence(now);
                
                absoluteExpiryTime = targetAbsoluteExpiryDateTime < nextExpiryTime
                    ? targetAbsoluteExpiryDateTime
                    : nextExpiryTime;
            }

            // Calculate an approximate size in bytes for this object. As there is no built-in mechanism
            // for determining the memory size of a C# object, this is a rough approximation.
            var json = JsonConvert.SerializeObject(item, null, _jsonSerializerSettings);
            var approximateSizeInBytes = Encoding.GetEncoding("utf-8").GetByteCount(json);

            var expiryTime = new DateTimeOffset(absoluteExpiryTime);

            _cache.Set(cacheKey, item, new MemoryCacheEntryOptions
            {
                Size = approximateSizeInBytes,
                AbsoluteExpiration = expiryTime
            });

            _logger.LogInformation("Setting cached item with cache key {CacheKeyDescription}, " +
                                   "approx size {Size} bytes, expiry time {ExpiryTime}",
                GetCacheKeyDescription(cacheKey), approximateSizeInBytes, expiryTime);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown when caching item with cache key {CacheKeyDescription}.  " +
                                "Returning gracefully.", 
                GetCacheKeyDescription(cacheKey));
            return Task.CompletedTask;
        }
    }

    private static string GetCacheKeyDescription(IMemoryCacheKey cacheKey)
    {
        return $"{cacheKey.GetType().Name} {cacheKey.Key}";
    }
}
