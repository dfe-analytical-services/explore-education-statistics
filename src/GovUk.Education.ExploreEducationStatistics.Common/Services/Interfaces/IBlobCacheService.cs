﻿#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobCacheService
    {
        Task<object?> GetItem(IBlobCacheKey cacheKey, Type targetType);

        Task SetItem<TItem>(IBlobCacheKey cacheKey, TItem item);

        Task DeleteItem(IBlobCacheKey cacheKey);
        
        Task DeleteCacheFolder(IBlobCacheKey cacheFolderKey);
    }
}
