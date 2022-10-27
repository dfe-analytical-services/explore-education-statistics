using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

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
            var filterGroup = LookupFilterGroup(filter, filterGroupLabel, context);
            return LookupFilterItem(filterGroup, filterItemLabel, context);
        }

        public FilterItem LookupFilterItem(FilterGroup filterGroup, string label, StatisticsDbContext context)
        {
            var cacheKey = GetFilterItemCacheKey( filterGroup, label, context);
            
            return _memoryCache.GetOrCreate(
                cacheKey, 
                () => context
                    .FilterItem
                    .AsNoTracking()
                    .First(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label));
        }

        public FilterGroup LookupFilterGroup(Filter filter, string label, StatisticsDbContext context)
        {
            var cacheKey = GetFilterGroupCacheKey(filter, label, context);

            return _memoryCache.GetOrCreate(
                cacheKey, 
                () => context
                    .FilterGroup
                    .AsNoTracking()
                    .First(fg => fg.FilterId == filter.Id && fg.Label == label));
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
}
