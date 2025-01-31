using System;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class DataServiceMemoryCache<TItem>
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public TItem GetOrCreate(object key, Func<ICacheEntry, TItem> factory)
        {
            return _cache.GetOrCreate(key, factory);
        }

        public TItem GetOrDefault(object key)
        {
            return _cache.Get<TItem>(key);
        }

        public TItem Set(object key, TItem item)
        {
            return _cache.Set(key, item);
        }

        public TItem Set(object key, TItem item, TimeSpan absoluteExpirationRelativeToNow)
        {
            return _cache.Set(key, item, absoluteExpirationRelativeToNow);
        }
    }
}