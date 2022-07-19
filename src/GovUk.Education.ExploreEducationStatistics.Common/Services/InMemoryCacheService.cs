#nullable enable
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.ExpirySchedule;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class InMemoryCacheService : IInMemoryCacheService
    {
        private const int MaxCacheSizeInMb = 50;
    
        private readonly JsonSerializerSettings _jsonSerializerSettings =
            GetJsonSerializerSettings(new CamelCaseNamingStrategy());
        
        private IMemoryCache _cache;
        private readonly ILogger<InMemoryCacheService> _logger;
        
        public InMemoryCacheService(
            ILogger<InMemoryCacheService> logger)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = MaxCacheSizeInMb * 1000000,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
            });
            _logger = logger;
        }

        public Task<object?> GetItem(IInMemoryCacheKey cacheKey, Type targetType)
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
            IInMemoryCacheKey cacheKey,
            TItem item,
            InMemoryCacheConfiguration configuration,
            DateTime? nowUtc = null)
        {
            var now = nowUtc ?? DateTime.UtcNow;

            DateTime absoluteExpiryTime;

            if (configuration.ExpirySchedule == None)
            {
                absoluteExpiryTime = now.AddSeconds(configuration.CacheDurationInSeconds);
            }
            else
            {
                var midnightToday = now.Date;
                var targetAbsoluteExpiryDateTime = now.AddSeconds(configuration.CacheDurationInSeconds);

                var expiryWindowStartTimesToday = configuration.GetDailyExpiryStartTimesInSeconds()
                    .Select(milliseconds => midnightToday.AddSeconds(milliseconds))
                    .ToList();

                var midnightTomorrow = midnightToday.AddDays(1);
                var nextExpiryWindowStart = expiryWindowStartTimesToday
                    .FirstOrDefault(expiryWindowStart => expiryWindowStart > now, midnightTomorrow);

                absoluteExpiryTime = targetAbsoluteExpiryDateTime < nextExpiryWindowStart 
                    ? targetAbsoluteExpiryDateTime 
                    : nextExpiryWindowStart;
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

        private static string GetCacheKeyDescription(IInMemoryCacheKey cacheKey)
        {
            return $"{cacheKey.GetType().Name} {cacheKey.Key}";
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategy namingStrategy)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy,
                },
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public void SetMemoryCache(IMemoryCache cache)
        {
            this._cache = cache;
        }
    }
}
