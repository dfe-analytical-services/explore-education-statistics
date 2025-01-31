#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class FilterItemRepository : IFilterItemRepository
    {
        private readonly StatisticsDbContext _context;

        public FilterItemRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds)
        {
            var filterItems = await _context.FilterItem
                .Include(filterItem => filterItem.FilterGroup)
                .Where(filterItem => filterItemIds.Contains(filterItem.Id))
                .ToListAsync();

            var notFound = filterItemIds.Where(id => filterItems.All(found => found.Id != id))
                .Select(filterItemId => filterItemId.ToString())
                .ToList();

            if (notFound.Any())
            {
                throw new ArgumentException($"Could not find filter items: {notFound.JoinToString(", ")}");
            }

            return filterItems
                .GroupBy(item => item.FilterGroup.FilterId)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
        }

        public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
            Guid subjectId,
            IQueryable<MatchedObservation> matchedObservations)
        {
            var matchedObservationIds = matchedObservations.Select(o => o.Id);

            var filtersForSubject = await _context
                .Filter
                .AsNoTracking()
                .Include(filter => filter.FilterGroups)
                .Where(filter => filter.SubjectId == subjectId)
                .ToListAsync();

            var filterGroupIds = filtersForSubject
                .SelectMany(filter => filter.FilterGroups)
                .Select(filterGroup => filterGroup.Id);

            var filterItems = await _context
                .FilterItem
                .AsNoTracking()
                .Where(filterItem =>
                    filterGroupIds.Contains(filterItem.FilterGroupId) &&
                    _context.ObservationFilterItem.Any(ofi =>
                        ofi.FilterItemId == filterItem.Id && matchedObservationIds.Contains(ofi.ObservationId)))
                .ToListAsync();

            var filterGroupsById = filtersForSubject
                .SelectMany(filter => filter.FilterGroups)
                .ToDictionary(filterGroup => filterGroup.Id);

            filterItems.ForEach(filterItem => filterItem.FilterGroup = filterGroupsById[filterItem.FilterGroupId]);

            return filterItems;
        }

        public async Task<IList<FilterItem>> GetFilterItemsFromObservations(IEnumerable<Observation> observations)
        {
            var filterItemIds =
                observations
                    .SelectMany(observation => observation.FilterItems)
                    .Select(ofi => ofi.FilterItemId)
                    .Distinct()
                    .ToList();

            return await _context
                .FilterItem
                .AsNoTracking()
                .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Where(fi => filterItemIds.Contains(fi.Id))
                .ToListAsync();
        }
    }
}
