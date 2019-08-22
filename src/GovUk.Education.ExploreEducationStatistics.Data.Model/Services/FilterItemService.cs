using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterItemService : AbstractRepository<FilterItem, long>, IFilterItemService
    {
        public FilterItemService(ApplicationDbContext context,
            ILogger<FilterItemService> logger) : base(context, logger)
        {
        }

        public IEnumerable<FilterItem> GetFilterItems(IQueryable<Observation> observations)
        {
            return observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem).Distinct();
        }

        public IEnumerable<FilterItem> GetFilterItemsIncludingFilters(IQueryable<Observation> observations)
        {
            return observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem)
                .Distinct()
                .Include(item => item.FilterGroup.Filter);
        }

        public FilterItem GetTotal(IEnumerable<FilterItem> filterItems)
        {
            return GetTotalGroup(filterItems)?.FirstOrDefault(IsFilterItemTotal);
        }

        private static IEnumerable<FilterItem> GetTotalGroup(IEnumerable<FilterItem> filterItems)
        {
            var itemsGroupedByFilterGroup = filterItems.GroupBy(item => item.FilterGroup).ToList();
            //Return the group if there is only one, otherwise the 'Total' group if it exists
            return itemsGroupedByFilterGroup.Count == 1
                ? itemsGroupedByFilterGroup.First()
                : itemsGroupedByFilterGroup.FirstOrDefault(items => IsFilterGroupTotal(items.Key));
        }

        private static bool IsFilterItemTotal(FilterItem item)
        {
            return IsEqualToIgnoreCase(item.Label, "Total");
        }

        private static bool IsFilterGroupTotal(FilterGroup group)
        {
            return IsEqualToIgnoreCase(group.Label, "Total");
        }

        private static bool IsEqualToIgnoreCase(string value, string compareTo)
        {
            return value.Equals(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}