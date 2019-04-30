using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterFilterService
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;

        public ImporterFilterService(IMemoryCache cache, ApplicationDbContext context)
        {
            _cache = cache;
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
            return _context.FilterItem.Add(new FilterItem(label, filterGroup)).Entity;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label)
        {
            if (label == null)
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
            var filterGroup = _context.FilterGroup.Add(new FilterGroup(filter, label)).Entity;
            _context.SaveChanges();
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