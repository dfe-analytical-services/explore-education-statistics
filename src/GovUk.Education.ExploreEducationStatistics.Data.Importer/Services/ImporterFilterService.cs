using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterFilterService : BaseImporterService
    {
        private readonly ApplicationDbContext _context;

        public ImporterFilterService(ImporterMemoryCache cache, ApplicationDbContext context) : base(cache)
        {
            _context = context;
        }

        public FilterItem Find(string filterItemLabel, string filterGroupLabel, Filter filter)
        {
            var filterGroup = LookupOrCreateFilterGroup(filter, filterGroupLabel);
            return LookupOrCreateFilterItem(filterGroup, filterItemLabel);
        }

        private FilterItem LookupOrCreateFilterItem(FilterGroup filterGroup, string label)
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
            
            filterItem = _context.FilterItem.FirstOrDefault(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label) 
                         ??_context.FilterItem.Add(new FilterItem(label, filterGroup)).Entity;

            GetCache().Set(cacheKey, filterItem);
            
            return filterItem;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label)
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
            
            filterGroup = _context.FilterGroup
                          .FirstOrDefault(fg => fg.FilterId == filter.Id && fg.Label == label) 
                          ?? _context.FilterGroup.Add(new FilterGroup(filter, label)).Entity;

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