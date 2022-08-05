#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;

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

        private static IConfigurationSection? _configuration;

        protected override Type BaseKey => typeof(IMemoryCacheKey);
        
        private int CacheDurationInSeconds { get; }
        
        private CronExpression? ExpirySchedule { get; }

        /// <summary>
        /// Specify a service to use <see cref="Services"/>.
        /// Otherwise, we use the first registered service.
        /// </summary>
        public string? ServiceName { get; set; }

        public MemoryCacheAttribute(
            Type key, 
            int cacheDurationInSeconds, 
            string? expiryScheduleCron = null
            ) : base(key)
        {
            CacheDurationInSeconds = cacheDurationInSeconds;
            ExpirySchedule = expiryScheduleCron != null ? CronExpression.Parse(expiryScheduleCron) : null;
        }

        public MemoryCacheAttribute(Type key, string cacheConfigKey) : base(key)
        {
            if (_configuration == null)
            {
                return;
            }
            
            var cacheConfiguration = _configuration.GetSection(cacheConfigKey);

            if (cacheConfiguration == null)
            {
                throw new ArgumentException($"Could not find MemoryCache.Configurations entry with key {cacheConfigKey}");
            }
            
            var cacheDuration = cacheConfiguration.GetValue<int>("CacheDurationInSeconds", 0);

            if (0 == cacheDuration)
            {
                throw new ArgumentException("A value for configuration " +
                                            "MemoryCache.Configurations.CacheDurationInSeconds must be specified");
            }

            CacheDurationInSeconds = cacheDuration;

            var expirySchedule = cacheConfiguration.GetValue<string>("ExpirySchedule", "");

            if ("" != expirySchedule)
            {
                ExpirySchedule = CronExpression.Parse(expirySchedule);
            }
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

        public static void SetConfiguration(IConfigurationSection? configurationSection)
        {
            _configuration = configurationSection;
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

                var itemCachingConfiguration = new MemoryCacheConfiguration(CacheDurationInSeconds, ExpirySchedule);
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