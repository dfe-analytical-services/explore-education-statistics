#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobCacheService : ICacheService<IBlobCacheKey>
    {
        Task<TItem> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<TItem> itemSupplier)
            where TItem : class;

        Task<TItem> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<Task<TItem>> itemSupplier)
            where TItem : class;

        Task<Either<ActionResult, TItem>> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<Task<Either<ActionResult, TItem>>> itemSupplier)
            where TItem : class;
    }
}
