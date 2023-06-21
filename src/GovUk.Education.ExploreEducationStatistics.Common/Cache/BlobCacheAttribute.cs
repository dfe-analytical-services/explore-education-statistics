#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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

        private StaleCacheWorkflow<BlobCacheKeyAndType, BlobCacheAttribute> _staleCacheWorkflow;

        public BlobCacheAttribute(Type key, bool forceUpdate = false) : base(key, forceUpdate)
        {
            _staleCacheWorkflow = new(
                cacheKey => GetAsync(cacheKey.CacheKey, cacheKey.Type),
                cacheKey => GetCacheItemMeta(cacheKey.CacheKey),
                (cacheKey, item) => SetAsync(cacheKey.CacheKey, item));
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
            if (cacheKey is IBlobCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return null;
                }

                return service.GetItem(key, returnType);
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
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

                return await service.GetItemAsync(key, returnType);
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        public override void Set(ICacheKey cacheKey, object value)
        {
            if (cacheKey is IBlobCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return;
                }

                service.SetItem(key, value);

                return;
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        public override async Task<object?> GetBlobMetaAsync(ICacheKey cacheKey, Type returnType)
        {
            if (cacheKey is IBlobCacheKey key)
            {
                var service = GetService();

                if (service is null)
                {
                    return null;
                }

                return await service.GetBlobMetaAsync(key, returnType);
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

                await service.SetItemAsync(key, value);

                return;
            }

            throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}");
        }

        protected override T GetOrGenerateAndSet<T>(ICacheKey cacheKey, Func<object[], T> target, object[] args)
        {
            throw new NotImplementedException();
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

    record BlobCacheKeyAndType(IBlobCacheKey CacheKey, Type Type);
}