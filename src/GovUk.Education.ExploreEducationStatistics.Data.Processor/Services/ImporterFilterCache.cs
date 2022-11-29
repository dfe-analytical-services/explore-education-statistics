#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterFilterCache
{
    private MemoryCache Cache { get; set; } = new(new MemoryCacheOptions
    {
        SizeLimit = 50000,
        ExpirationScanFrequency = TimeSpan.FromMinutes(5)
    });
    
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
    {
        Size = 1,
        SlidingExpiration = TimeSpan.FromMinutes(1)
    };
    
    public FilterItem GetOrCacheFilterItem(
        Guid filterGroupId, 
        string filterItemLabel, 
        Func<FilterItem> filterItemProvider)
    {
        return GetOrCreate(
            GetFilterItemCacheKey(filterGroupId, filterItemLabel), 
            filterItemProvider);
    }
    
    public FilterGroup GetOrCacheFilterGroup(
        Guid filterId, 
        string filterGroupLabel, 
        Func<FilterGroup> filterGroupProvider)
    {
        return GetOrCreate(
            GetFilterGroupCacheKey(filterId, filterGroupLabel), 
            filterGroupProvider);
    }
    
    public void AddFilterGroup(FilterGroup filterGroup)
    {
        Set(GetFilterGroupCacheKey(filterGroup.FilterId, filterGroup.Label), filterGroup);
    }
    
    public void AddFilterItem(FilterItem filterItem)
    {
        Set(GetFilterItemCacheKey(filterItem.FilterGroupId, filterItem.Label), filterItem);
    }

    private void Set<TItem>(object cacheKey, TItem cacheItem)
    {
        Cache.Set(cacheKey, cacheItem, _cacheEntryOptions);
    }

    private TItem GetOrCreate<TItem>(object cacheKey, Func<TItem> defaultItemProvider)
    {
        return Cache.GetOrCreate(cacheKey, entry =>
        {
            var defaultItem = defaultItemProvider.Invoke();
            entry.SetOptions(_cacheEntryOptions);
            entry.Value = defaultItem;
            return defaultItem;
        });
    }

    private static string GetFilterGroupCacheKey(Guid filterId, string filterGroupLabel)
    {
        return nameof(FilterGroup) + "_" +
               filterId + "_" +
               filterGroupLabel.ToLower().Replace(" ", "_");            
    }

    private static string GetFilterItemCacheKey(Guid filterGroupId, string filterItemLabel)
    {
        return nameof(FilterItem) + "_" +
               filterGroupId + "_" +
               filterItemLabel.ToLower().Replace(" ", "_");
    }
}