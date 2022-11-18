#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
        FilterGroup filterGroup, 
        string filterItemLabel, 
        StatisticsDbContext context,
        Func<FilterItem> filterItemProvider)
    {
        return GetOrCreate(
            GetFilterItemCacheKey(filterGroup, filterItemLabel, context), 
            filterItemProvider);
    }
    
    public FilterGroup GetOrCacheFilterGroup(
        Filter filter, 
        string filterGroupLabel, 
        StatisticsDbContext context,
        Func<FilterGroup> filterGroupProvider)
    {
        return GetOrCreate(
            GetFilterGroupCacheKey(filter, filterGroupLabel, context), 
            filterGroupProvider);
    }
    
    public void AddFilterGroup(FilterGroup filterGroup, StatisticsDbContext context)
    {
        Set(GetFilterGroupCacheKey(filterGroup.Filter, filterGroup.Label, context), filterGroup);
    }
    
    public void AddFilterItem(FilterItem filterItem, StatisticsDbContext context)
    {
        Set(GetFilterItemCacheKey(filterItem.FilterGroup, filterItem.Label, context), filterItem);
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

    private static string GetFilterGroupCacheKey(Filter filter, string filterGroupLabel, StatisticsDbContext context)
    {
        return nameof(FilterGroup) + "_" +
               (filter.Id == null ? context.Entry(filter).Property(e => e.Id).CurrentValue : filter.Id) + "_" +
               filterGroupLabel.ToLower().Replace(" ", "_");            
    }

    private static string GetFilterItemCacheKey(FilterGroup filterGroup, string filterItemLabel, StatisticsDbContext context)
    {
        return nameof(FilterItem) + "_" +
               (filterGroup.Id == null ? context.Entry(filterGroup).Property(e => e.Id).CurrentValue : filterGroup.Id) + "_" +
               filterItemLabel.ToLower().Replace(" ", "_");
    }
}