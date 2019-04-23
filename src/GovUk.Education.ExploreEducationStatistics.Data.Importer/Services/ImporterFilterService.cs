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

        private FilterItem LookupOrCreateFilterItem(FilterGroup filterGroup, string filterItemLabel)
        {
            var cacheKey = filterGroup.Id + "_" + filterItemLabel;
            if (_cache.TryGetValue(cacheKey, out FilterItem filterItem))
            {
                return filterItem;
            }

            filterItem = CreateFilterItem(filterGroup, filterItemLabel);
            _cache.Set(cacheKey, filterItem,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return filterItem;
        }

        private FilterItem CreateFilterItem(FilterGroup filterGroup, string filterItemLabel)
        {
            return _context.FilterItem.Add(new FilterItem
            {
                Label = filterItemLabel,
                FilterGroup = filterGroup
            }).Entity;
        }

        private FilterGroup LookupOrCreateFilterGroup(Filter filter, string label)
        {
            if (label == null)
            {
                label = "All";
            }

            var cacheKey = filter.Id + "_" + label;
            if (_cache.TryGetValue(cacheKey, out FilterGroup filterGroup))
            {
                return filterGroup;
            }

            filterGroup = CreateFilterGroup(filter, label);
            _cache.Set(cacheKey, filterGroup,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return filterGroup;
        }

        private FilterGroup CreateFilterGroup(Filter filter, string label)
        {
            return _context.FilterGroup.Add(new FilterGroup
            {
                Filter = filter,
                Label = label
            }).Entity;
        }
    }
}