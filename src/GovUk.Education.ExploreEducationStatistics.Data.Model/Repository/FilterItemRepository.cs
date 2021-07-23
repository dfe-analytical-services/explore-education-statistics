using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class FilterItemRepository : AbstractRepository<FilterItem, Guid>, IFilterItemRepository
    {
        public FilterItemRepository(StatisticsDbContext context) : base(context)
        {
        }

        public IEnumerable<FilterItem> GetFilterItems(Guid subjectId, IQueryable<Observation> observations, bool listFilterItems)
        {
            // Temporary measure hopefully!
            // The following query is optimal but since IQueryable observations can contain n number of conditions then LINQ
            // may not be capable of converting it so allow a less efficient query to be executed

            if (!listFilterItems)
            {
                // optimal query
                return _context.FilterItem
                    .Include(fi => fi.FilterGroup)
                    .ThenInclude(fg => fg.Filter).AsNoTracking()
                    .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId &&
                           observations.Any(o => o.FilterItems.Any(
                               ofi => ofi.FilterItemId == fi.Id)));
            }

            // sub-optimal query
            var allFilterItemsForSubject = _context.FilterItem
                .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter).AsNoTracking()
                .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
                .ToList();

            return allFilterItemsForSubject.Where(
                fi => observations.Any(o => o.FilterItems.Any(
                    ofi => ofi.FilterItemId == fi.Id)));
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
            var itemsGroupedByFilterGroup = filterItems.GroupBy(item => item.FilterGroup, FilterGroup.IdComparer).ToList();

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