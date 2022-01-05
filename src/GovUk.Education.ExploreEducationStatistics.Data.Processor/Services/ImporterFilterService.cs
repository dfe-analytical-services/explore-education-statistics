using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterFilterService
    {
        private readonly ImporterMemoryCache _memoryCache;

        public ImporterFilterService(ImporterMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public FilterItem Find(string filterItemLabel, string filterGroupLabel, Filter filter, StatisticsDbContext context)
        {
            var filterGroup = LookupOrCreateFilterGroup(filter, filterGroupLabel, context);
            return LookupOrCreateFilterItem(filter, filterGroup, filterItemLabel, context);
        }

        private FilterItem LookupOrCreateFilterItem(Filter filter, FilterGroup filterGroup, string label, StatisticsDbContext context)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Not specified";
            }

            var cacheKey = GetFilterItemCacheKey( filterGroup, label, context);
            if (_memoryCache.Cache.TryGetValue(cacheKey, out FilterItem filterItem))
            {
                return filterItem;
            }

            filterItem = context.FilterItem.AsNoTracking().FirstOrDefault(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label) 
                         ?? context.FilterItem.Add(new FilterItem(label, filterGroup)).Entity;

            _memoryCache.Cache.Set(cacheKey, filterItem);
            
            return filterItem;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label, StatisticsDbContext context)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Default";
            }

            var cacheKey = GetFilterGroupCacheKey(filter, label, context);
            if (_memoryCache.Cache.TryGetValue(cacheKey, out FilterGroup filterGroup))
            {
                return filterGroup;
            }

            filterGroup = context.FilterGroup.AsNoTracking()
                          .FirstOrDefault(fg => fg.FilterId == filter.Id && fg.Label == label)
                          ?? context.FilterGroup.Add(new FilterGroup(filter, label)).Entity;
            
            _memoryCache.Cache.Set(cacheKey, filterGroup);

            return filterGroup;
        }

        private static string GetFilterGroupCacheKey(Filter filter, string filterGroupLabel, StatisticsDbContext context)
        {
            return typeof(FilterGroup).Name + "_" +
                   (filter.Id == null ? context.Entry(filter).Property(e => e.Id).CurrentValue : filter.Id) + "_" +
                   filterGroupLabel.ToLower().Replace(" ", "_");            
        } 

        private static string GetFilterItemCacheKey(FilterGroup filterGroup, string filterItemLabel, StatisticsDbContext context)
        {
            return typeof(FilterItem).Name + "_" +
                   (filterGroup.Id == null ? context.Entry(filterGroup).Property(e => e.Id).CurrentValue : filterGroup.Id) + "_" +
                   filterItemLabel.ToLower().Replace(" ", "_");
        }
    }
}
