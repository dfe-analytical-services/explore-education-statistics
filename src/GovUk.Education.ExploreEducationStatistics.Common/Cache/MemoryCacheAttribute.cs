#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public class MemoryCacheAttribute : CacheAttribute
    {
        /// <summary>
        /// A Dictionary of available IMemoryCacheService implementations. It is possible here to register
        /// MemoryCacheServices that can each handle a different cache and with a different configuration. Therefore
        /// we could register services that handle short-lived caches, caches that never expire etc, and the individual
        /// [MemoryCache] attributes on methods could identify which cache service they need via the "ServiceName"
        /// parameter.
        /// </summary>
        private static Dictionary<string, IMemoryCacheService> Services { get; set; } = new();

        private static int? _overrideDurationInSeconds;

        private static CrontabSchedule? _overrideExpirySchedule;

        protected override Type BaseKey => typeof(IMemoryCacheKey);
        
        private int DurationInSeconds { get; }
        
        private CrontabSchedule? ExpirySchedule { get; }

        /// <summary>
        /// Specify a service to use <see cref="Services"/>.
        /// Otherwise, we use the first registered service.
        /// </summary>
        public string? ServiceName { get; set; }

        public MemoryCacheAttribute(
            Type key, 
            int durationInSeconds, 
            string? expiryScheduleCron = null,
            bool forceUpdate = false
        ) : base(key, forceUpdate)
        {
            DurationInSeconds = durationInSeconds;
            ExpirySchedule = expiryScheduleCron != null ? CrontabSchedule.Parse(expiryScheduleCron) : null;
        }

        public static void AddService(string name, IMemoryCacheService service)
        {
            Services[name] = service;
        }
        public static void ClearServices()
        {
            Services.Clear();
        }

        public static void SetOverrideConfiguration(IConfigurationSection? configurationSection)
        {
            var overrideDurationInSeconds = configurationSection?.GetValue<int?>("DurationInSeconds");

            _overrideDurationInSeconds = overrideDurationInSeconds != null && overrideDurationInSeconds != -1
                ? overrideDurationInSeconds.Value : null;

            var overrideExpirySchedule = configurationSection?.GetValue<string?>("ExpirySchedule");

            _overrideExpirySchedule = !overrideExpirySchedule.IsNullOrEmpty()
                ? CrontabSchedule.Parse(overrideExpirySchedule) : null;
        }

        public override object? Get(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is not IMemoryCacheKey key)
            {
                throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
            }

            var service = GetService();

            return service?.GetItem(key, returnType);
        }

        public override Task<object?> GetAsync(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is not IMemoryCacheKey key)
            {
                throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
            }

            var service = GetService();

            if (service is null)
            {
                return Task.FromResult<object?>(null);
            }

            var cachedItem = service.GetItem(key, returnType);
            return Task.FromResult(cachedItem);
        }

        public override void Set(ICacheKey cacheKey, object value)
        {
            if (cacheKey is not IMemoryCacheKey key)
            {
                throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
            }

            var service = GetService();

            service?.SetItem(key, value,
                new MemoryCacheConfiguration(
                    _overrideDurationInSeconds ?? DurationInSeconds,
                    _overrideExpirySchedule ?? ExpirySchedule));
        }

        public override Task SetAsync(ICacheKey cacheKey, object value)
        {
            if (cacheKey is not IMemoryCacheKey key)
            {
                throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
            }

            var service = GetService();

            if (service is null)
            {
                return Task.CompletedTask;
            }

            var itemCachingConfiguration = new MemoryCacheConfiguration(
                _overrideDurationInSeconds ?? DurationInSeconds,
                _overrideExpirySchedule ?? ExpirySchedule);
            service.SetItem(key, value, itemCachingConfiguration);
            return Task.CompletedTask;
        }

        private IMemoryCacheService? GetService()
        {
            if (ServiceName is not null)
            {
                return Services[ServiceName];
            }

            return Services.Count > 0 ? Services.First().Value : null;
        }
    }
}
