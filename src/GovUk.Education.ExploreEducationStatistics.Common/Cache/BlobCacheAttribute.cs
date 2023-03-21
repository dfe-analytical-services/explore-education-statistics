#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public class BlobCacheAttribute : CacheAttribute
    {
        private static Dictionary<string, IBlobCacheService> Services { get; set; } = new();

        protected override Type BaseKey => typeof(IBlobCacheKey);

        /// <summary>
        /// Specify a service to use <see cref="Services"/>.
        /// Otherwise, we use the first registered service.
        /// </summary>
        public string? ServiceName { get; set; }

        public BlobCacheAttribute(Type key, bool forceUpdate = false) : base(key, forceUpdate)
        {
        }

        public static void AddService(string name, IBlobCacheService service)
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

        public override object? Get(ICacheKey cacheKey, Type returnType)
        {
            // Blob cache requires an async call to blob storage, so allowing use of `Get` would inject async code into
            // the sync attributed method.
            throw new ArgumentException("The BlobCache attribute cannot be applied to sync methods");
        }

        public override void Set(ICacheKey cacheKey, object value)
        {
            // Blob cache requires an async call to blob storage, so allowing use of `Set` would inject async code into
            // the sync attributed method.
            throw new ArgumentException("The BlobCache attribute cannot be applied to sync methods");
        }

        public override async Task<object?> GetAsync(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is IBlobCacheKey key)
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

        public override async Task SetAsync(ICacheKey cacheKey, object value)
        {
            if (cacheKey is IBlobCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return;
                }

                await service.SetItem(key, value);

                return;
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        private IBlobCacheService? GetService()
        {
            if (ServiceName is not null)
            {
                return Services[ServiceName];
            }

            return Services.Count > 0 ? Services.First().Value : null;
        }
    }
}
