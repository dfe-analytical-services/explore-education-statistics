#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public override Task<object?> GetAsync(ICacheKey cacheKey, Type returnType)
        {
            return WithCacheKeyAndService(cacheKey, (key, service) => service.GetItemAsync(key, returnType));
        }

        public override void Set(ICacheKey cacheKey, object value)
        {
            WithCacheKeyAndService(cacheKey, (key, service) => service.SetItem(key, value));
        }

        public override Task SetAsync(ICacheKey cacheKey, object value)
        {
            return WithCacheKeyAndService(cacheKey, (key, service) => service.SetItemAsync(key, value));
        }

        protected override Task<object> GetOrCreateAndCacheItemAsync(object cacheKey, Type returnType, Func<Task<object?>> createItemFn)
        {
            return WithCacheKeyAndService(cacheKey, (key, service) => 
                service.GetOrCreateAndCacheItemAsync(key, returnType, createItemFn)); 
        }
        
        private Task<T?> WithCacheKeyAndService<T>(object cacheKey, Func<IBlobCacheKey, IBlobCacheService, Task<T?>> func) where T : class
        {
            if (!(cacheKey is IBlobCacheKey key))
            {
                throw new ArgumentException($"Cache key must by assignable to {BaseKey.GetPrettyFullName()}"); 
            }

            var service = GetService();

            if (service is null)
            {
                return null;
            }

            return func.Invoke(key, service);
        }
        
        private async Task WithCacheKeyAndService(object cacheKey, Func<IBlobCacheKey, IBlobCacheService, Task> func)
        {
            await WithCacheKeyAndService(cacheKey, async (key, service) =>
            {
                await func.Invoke(key, service);
                return Unit.Instance;
            });
        }
        
        private void WithCacheKeyAndService(object cacheKey, Action<IBlobCacheKey, IBlobCacheService> action)
        {
            WithCacheKeyAndService(cacheKey, async (key, service) =>
            {
                action.Invoke(key, service);
                return Unit.Instance;
            });
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