#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService<in TKey> where TKey : ICacheKey
    {
        Task<TItem?> GetItem<TItem>(TKey cacheKey)
            where TItem : class;

        Task<object?> GetItem(TKey cacheKey, Type targetType);

        Task SetItem<TItem>(TKey cacheKey, TItem item);

        Task DeleteItem(TKey cacheKey);

        Task DeleteCacheFolder(TKey cacheFolderKey);
    }
}