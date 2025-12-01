using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class MemoryCacheServiceExtensions
{
    public static async Task<TResult> GetOrCreateAsync<TResult, TLogger>(
        this IMemoryCacheService service,
        IMemoryCacheKey cacheKey,
        Func<Task<TResult>> createIfNotExistsFn,
        int durationInSeconds,
        TimeProvider timeProvider,
        ILogger<TLogger> logger,
        string? expiryScheduleCron = null
    )
        where TLogger : class
        where TResult : class
    {
        var options = service.GetMemoryCacheOptions();

        if (options is null || !options.Enabled)
        {
            return await createIfNotExistsFn();
        }

        var unboxedResultType = typeof(TResult).GetUnboxedResultTypePath().Last();

        var existingCacheEntry = service.GetItem(cacheKey: cacheKey, targetType: unboxedResultType);

        if (existingCacheEntry != null)
        {
            if (existingCacheEntry.TryBoxToResult(typeof(TResult), out var boxedResult))
            {
                return (TResult)boxedResult!;
            }

            logger.LogWarning(
                "Unable to box result of cached type {UnboxedType} to {BoxedType} with cache key {CacheKey}. "
                    + "Bypassing caching and returning fresh result.",
                existingCacheEntry.GetType().Name,
                typeof(TResult).Name,
                cacheKey
            );
        }

        return await service.CreateAndSetCacheEntry(
            cacheKey: cacheKey,
            createIfNotExistsFn: createIfNotExistsFn,
            durationInSeconds: durationInSeconds,
            expiryScheduleCron: expiryScheduleCron,
            timeProvider: timeProvider,
            logger: logger
        );
    }

    private static async Task<TResult> CreateAndSetCacheEntry<TResult, TLogger>(
        this IMemoryCacheService service,
        IMemoryCacheKey cacheKey,
        Func<Task<TResult>> createIfNotExistsFn,
        int durationInSeconds,
        TimeProvider timeProvider,
        ILogger<TLogger> logger,
        string? expiryScheduleCron = null
    )
        where TLogger : class
        where TResult : class?
    {
        var newCacheEntry = await createIfNotExistsFn.Invoke();

        if (newCacheEntry == null)
        {
            logger.LogWarning(
                "Cacheable result of type {ResultType} with cache key {CacheKey} is null. Not adding to cache.",
                typeof(TResult).Name,
                cacheKey
            );
            return null!;
        }

        if (!newCacheEntry.TryUnboxResult(out var unboxedEntry))
        {
            logger.LogWarning(
                "Unable to unbox type of new result {BoxedType} to an unboxed type for caching with cache key {CacheKey}. "
                    + "Bypassing setting this object in the cache.",
                newCacheEntry.GetType().Name,
                cacheKey
            );
            return newCacheEntry;
        }

        var options = service.GetMemoryCacheOptions();

        var duration = options?.Overrides?.DurationInSeconds ?? durationInSeconds;

        var crontabSchedule = expiryScheduleCron != null ? CrontabSchedule.Parse(expiryScheduleCron) : null;

        service.SetItem(
            cacheKey: cacheKey,
            item: unboxedEntry,
            configuration: new MemoryCacheConfiguration(DurationInSeconds: duration, ExpirySchedule: crontabSchedule),
            nowUtc: timeProvider.GetUtcNow()
        );

        return newCacheEntry;
    }
}
