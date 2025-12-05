using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class MemoryCacheService(
    IMemoryCache cache,
    MemoryCacheServiceOptions options,
    ILogger<MemoryCacheService> logger
) : IMemoryCacheService
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
    };

    public object? GetItem(IMemoryCacheKey cacheKey, Type targetType)
    {
        object? cachedItem;

        try
        {
            cachedItem = cache.Get<object?>(cacheKey);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error whilst retrieving cached item for key {CacheKey} - returning null",
                GetCacheKeyDescription(cacheKey)
            );
            return null;
        }

        if (cachedItem == null)
        {
            logger.LogInformation("Cache miss for cache key {CacheKeyDescription}", GetCacheKeyDescription(cacheKey));
            return null;
        }

        if (!targetType.IsInstanceOfType(cachedItem))
        {
            logger.LogError(
                "Cached type {CachedItemType} is not an instance of "
                    + "{TargetType} for cache key {CacheKey} - returning null",
                cachedItem.GetType(),
                nameof(targetType),
                GetCacheKeyDescription(cacheKey)
            );

            return null;
        }

        logger.LogInformation(
            "Returning cached result for cache key {CacheKeyDescription}",
            GetCacheKeyDescription(cacheKey)
        );

        return cachedItem;
    }

    public void SetItem<TItem>(
        IMemoryCacheKey cacheKey,
        TItem item,
        MemoryCacheConfiguration configuration,
        DateTimeOffset nowUtc
    )
    {
        try
        {
            DateTimeOffset absoluteExpiryTime;
            var targetAbsoluteExpiryDateTime = nowUtc.AddSeconds(configuration.DurationInSeconds);

            if (configuration.ExpirySchedule == null)
            {
                absoluteExpiryTime = targetAbsoluteExpiryDateTime;
            }
            else
            {
                var nextExpiryTime = configuration.ExpirySchedule.GetNextOccurrence(nowUtc.UtcDateTime);

                absoluteExpiryTime =
                    targetAbsoluteExpiryDateTime < nextExpiryTime ? targetAbsoluteExpiryDateTime : nextExpiryTime;
            }

            // Calculate an approximate size in bytes for this object. As there is no built-in mechanism
            // for determining the memory size of a C# object, this is a rough approximation.
            var json = JsonConvert.SerializeObject(item, null, _jsonSerializerSettings);
            var approximateSizeInBytes = Encoding.GetEncoding("utf-8").GetByteCount(json);

            cache.Set(
                cacheKey,
                item,
                new MemoryCacheEntryOptions { Size = approximateSizeInBytes, AbsoluteExpiration = absoluteExpiryTime }
            );

            logger.LogInformation(
                "Setting cached item with cache key {CacheKeyDescription}, "
                    + "approx size {Size} bytes, expiry time {AbsoluteExpiryTime}",
                GetCacheKeyDescription(cacheKey),
                approximateSizeInBytes,
                absoluteExpiryTime
            );
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Exception thrown when caching item with cache key {CacheKeyDescription}.  " + "Returning gracefully.",
                GetCacheKeyDescription(cacheKey)
            );
        }
    }

    // TODO EES-6450 - stop exposing this publicly when "GetOrCreate" method is absorbed into this service.
    public MemoryCacheServiceOptions? GetMemoryCacheOptions()
    {
        return options;
    }

    private static string GetCacheKeyDescription(IMemoryCacheKey cacheKey)
    {
        return $"{cacheKey.GetType().Name} {cacheKey.Key}";
    }
}
