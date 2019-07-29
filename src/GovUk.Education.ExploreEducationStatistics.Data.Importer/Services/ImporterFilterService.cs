using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterFilterService
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCache _cache;

        public ImporterFilterService(MyMemoryCache cache, ApplicationDbContext context)
        {
             _cache = cache.Cache;
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
            if (_cache.TryGetValue(cacheKey, out FilterItem filterItem))
            {
                return filterItem;
            }
            
            // TODO change expiry or introduce lookup

            filterItem = CreateFilterItem(filterGroup, label);
      
            _cache.Set(cacheKey, filterItem,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
            
            return filterItem;
        }

        private FilterItem CreateFilterItem(FilterGroup filterGroup, string label)
        {
            var filterItem = _context.FilterItem
                .FirstOrDefault(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label);
            
            if (filterItem == null)
            {
                filterItem = _context.FilterItem.Add(new FilterItem(label, filterGroup)).Entity;
            }

            return filterItem;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Default";
            }
            
            var cacheKey = GetFilterGroupCacheKey(filter, label);
            if (_cache.TryGetValue(cacheKey, out FilterGroup filterGroup))
            {
                return filterGroup;
            }
    
            // TODO change expiry or introduce lookup

            filterGroup = CreateFilterGroup(filter, label);
 
            _cache.Set(cacheKey, filterGroup,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
            
            return filterGroup;
        }

        private FilterGroup CreateFilterGroup(Filter filter, string label)
        {
            try
            {
                var filterGroup = _context.FilterGroup
                    .FirstOrDefault(fg => fg.FilterId == filter.Id && fg.Label == label);
                
                if (filterGroup == null)
                {
                    filterGroup = _context.FilterGroup.Add(new FilterGroup(filter, label)).Entity;
                }

                return filterGroup;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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