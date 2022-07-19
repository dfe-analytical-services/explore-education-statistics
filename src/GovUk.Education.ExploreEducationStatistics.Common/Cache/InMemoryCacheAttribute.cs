#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.ExpirySchedule;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public class InMemoryCacheAttribute : CacheAttribute
    {
        /**
         * A Dictionary of available IMemoryCacheService implementations. It is possible here to register
         * InMemoryCacheServices that can each handle a different cache and with a different configuration. Therefore
         * we could register services that handle short-lived caches, caches that never expire etc, and the individual
         * [InMemoryCache] attributes on methods could identify which cache service they need via the "ServiceName"
         * parameter.
         */
        private static Dictionary<string, IInMemoryCacheService> Services { get; set; } = new();

        protected override Type BaseKey => typeof(IInMemoryCacheKey);
        
        private int CacheDurationInSeconds { get; }
        
        private ExpirySchedule ExpirySchedule { get; }

        /// <summary>
        /// Specify a service to use <see cref="Services"/>.
        /// Otherwise, we use the first registered service.
        /// </summary>
        public string? ServiceName { get; set; }

        public InMemoryCacheAttribute(
            Type key, 
            int cacheDurationInSeconds, 
            ExpirySchedule expirySchedule = None
            ) : base(key)
        {
            CacheDurationInSeconds = cacheDurationInSeconds;
            ExpirySchedule = expirySchedule;
        }

        public static void AddService(string name, IInMemoryCacheService service)
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

        public override async Task<object?> Get(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is IInMemoryCacheKey key)
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
            if (cacheKey is IInMemoryCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return;
                }

                var itemCachingConfiguration = new InMemoryCacheConfiguration(ExpirySchedule, CacheDurationInSeconds);
                await service.SetItem(key, value, itemCachingConfiguration);
                return;
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        private IInMemoryCacheService? GetService()
        {
            if (ServiceName is not null)
            {
                return Services[ServiceName];
            }

            return Services.Count > 0 ? Services.First().Value : null;
        }
    }
}