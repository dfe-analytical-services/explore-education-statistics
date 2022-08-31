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

        private static int? OverrideDurationInSeconds;
        
        private static CrontabSchedule? OverrideExpirySchedule;

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
            DurationInSeconds = OverrideDurationInSeconds ?? durationInSeconds;
            ExpirySchedule = OverrideExpirySchedule ?? (
                expiryScheduleCron != null ? CrontabSchedule.Parse(expiryScheduleCron) : null);
        }

        public static void AddService(string name, IMemoryCacheService service)
        {
            Services[name] = service;
        }
        public static void RemoveService(string name)
        {
            Services.Remove(name);
        }

        public static void ClearServices()
        {
            Services.Clear();
        }

        public static void SetOverrideConfiguration(IConfigurationSection? configurationSection)
        {
            var durationInSeconds = configurationSection?.GetValue<int?>("DurationInSeconds");
            
            OverrideDurationInSeconds = durationInSeconds != null && durationInSeconds != -1 
                ? durationInSeconds.Value : null;
            
            var overrideExpirySchedule = configurationSection?.GetValue<string?>("ExpirySchedule");

            OverrideExpirySchedule = !overrideExpirySchedule.IsNullOrEmpty() 
                ? CrontabSchedule.Parse(overrideExpirySchedule) : null;
        }

        public override async Task<object?> Get(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is IMemoryCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return null;
                }

                return await service.GetItem(key, returnType);
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        public override async Task Set(ICacheKey cacheKey, object value)
        {
            if (cacheKey is IMemoryCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return;
                }

                var itemCachingConfiguration = new MemoryCacheConfiguration(DurationInSeconds, ExpirySchedule);
                await service.SetItem(key, value, itemCachingConfiguration);
                return;
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
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
