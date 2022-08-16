#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService<in TKey> where TKey : ICacheKey
    {
        Task<TItem?> GetItem<TItem>(TKey cacheKey)
            where TItem : class;

        Task<object?> GetItem(TKey cacheKey, Type targetType);

        Task SetItem<TItem>(TKey cacheKey, TItem item);

        Task DeleteItem(TKey cacheKey);
        
        Task<TItem> GetItem<TItem>(
            TKey cacheKey,
            Func<TItem> itemSupplier)
            where TItem : class;

        Task<TItem> GetItem<TItem>(
            TKey cacheKey,
            Func<Task<TItem>> itemSupplier)
            where TItem : class;

        Task<Either<ActionResult, TItem>> GetItem<TItem>(
            TKey cacheKey,
            Func<Task<Either<ActionResult, TItem>>> itemSupplier)
            where TItem : class;
    }
}