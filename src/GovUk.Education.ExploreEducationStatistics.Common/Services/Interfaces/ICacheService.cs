#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService<in TKey> where TKey : ICacheKey
    {
        object? GetItem(TKey cacheKey, Type targetType);

        Task<object?> GetItemAsync(TKey cacheKey, Type targetType);

        void SetItem<TItem>(TKey cacheKey, TItem item);

        Task SetItemAsync<TItem>(TKey cacheKey, TItem item);
    }
}
