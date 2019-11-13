using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterFilterService : BaseImporterService
    {
        public ImporterFilterService(ImporterMemoryCache cache) : base(cache)
        {
        }

        public FilterItem Find(string filterItemLabel, string filterGroupLabel, Filter filter, StatisticsDbContext context)
        {
            var filterGroup = LookupOrCreateFilterGroup(filter, filterGroupLabel, context);
            return LookupOrCreateFilterItem(filterGroup, filterItemLabel, context);
        }

        private FilterItem LookupOrCreateFilterItem(FilterGroup filterGroup, string label, StatisticsDbContext context)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Not specified";
            }

            var cacheKey = GetFilterItemCacheKey(filterGroup, label);
            if (GetCache().TryGetValue(cacheKey, out FilterItem filterItem))
            {
                return filterItem;
            }
            
            filterItem = context.FilterItem.AsNoTracking().FirstOrDefault(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label) 
                         ?? context.FilterItem.Add(new FilterItem(label, filterGroup)).Entity;
            
            GetCache().Set(cacheKey, filterItem);
            
            return filterItem;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label, StatisticsDbContext context)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Default";
            }
            
            var cacheKey = GetFilterGroupCacheKey(filter, label);
            if (GetCache().TryGetValue(cacheKey, out FilterGroup filterGroup))
            {
                return filterGroup;
            }
            
            filterGroup = context.FilterGroup.AsNoTracking()
                          .FirstOrDefault(fg => fg.FilterId == filter.Id && fg.Label == label) 
                          ?? context.FilterGroup.Add(new FilterGroup(filter, label)).Entity;
            
            GetCache().Set(cacheKey, filterGroup);
            
            return filterGroup;
        }

        private static string GetFilterGroupCacheKey(Filter filter, string filterGroupLabel)
        {
            return typeof(FilterGroup).Name + "_" +
                   filter.Id + "_" +
                   filterGroupLabel.ToLower().Replace(" ", "_");            
        } 
        
        private static string GetFilterItemCacheKey(FilterGroup filterGroup, string filterItemLabel)
        {
            return typeof(FilterItem).Name + "_" +
                   filterGroup.Id + "_" +
                   filterItemLabel.ToLower().Replace(" ", "_");
        }
    }
}