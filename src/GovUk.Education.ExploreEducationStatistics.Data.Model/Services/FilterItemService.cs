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
        public FilterItemService(StatisticsDbContext context,
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
            var filterItems = observations
                .Join(
                    _context.ObservationFilterItem, 
                    observation => observation,
                    observationFilterItem => observationFilterItem.Observation,
                    (observation, observationFilterItem) => observationFilterItem
                )
                .Join(
                    _context.FilterItem,
                    observationFilterItem => observationFilterItem.FilterItem,
                    filterItem => filterItem,
                    (observationFilterItem, filterItem) => filterItem
                )
                .Include(item => item.FilterGroup)
                .ThenInclude(group => group.Filter)
//                .Join(
//                    _context.FilterGroup,
//                    filterItem => filterItem.FilterGroup,
//                    filterGroup => filterGroup,
//                    (filterItem, filterGroup) => filterGroup
//                )
//                .Join(
//                    _context.Filter,
//                    filterGroup => filterGroup.Filter,
//                    filter => filter,
//                    (filterGroup, filter) => filter
//                )
                .ToList();

            return filterItems.Distinct();
        }

        public FilterItem GetTotal(Filter filter)
        {
            return GetTotalGroup(filter)?.FilterItems.FirstOrDefault(IsFilterItemTotal);
        }

        public FilterItem GetTotal(IEnumerable<FilterItem> filterItems)
        {
            return GetTotalGroup(filterItems)?.FirstOrDefault(IsFilterItemTotal);
        }

        private static IEnumerable<FilterItem> GetTotalGroup(IEnumerable<FilterItem> filterItems)
        {
            var itemsGroupedByFilterGroup = filterItems.GroupBy(item => item.FilterGroup).ToList();

            // Return the group if there is only one, otherwise the 'Total' group if it exists
            return itemsGroupedByFilterGroup.Count == 1
                ? itemsGroupedByFilterGroup.First()
                : itemsGroupedByFilterGroup.FirstOrDefault(items => IsFilterGroupTotal(items.Key));
        }

        private static FilterGroup GetTotalGroup(Filter filter)
        {
            var filterGroups = filter.FilterGroups;

            // Return the group if there is only one, otherwise the 'Total' group if it exists
            return filterGroups.Count() == 1
                ? filterGroups.First()
                : filterGroups.FirstOrDefault(IsFilterGroupTotal);
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