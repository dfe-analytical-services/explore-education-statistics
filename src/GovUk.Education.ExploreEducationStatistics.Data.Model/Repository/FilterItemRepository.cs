#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class FilterItemRepository(StatisticsDbContext context) : IFilterItemRepository
{
    public async Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds)
    {
        var filterItems = await context.FilterItem
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
        var matchedFilterItemIds = matchedObservations
            .Join(
                inner: context.ObservationFilterItem,
                outerKeySelector: observation => observation.Id,
                innerKeySelector: observationFilterItem => observationFilterItem.ObservationId,
                resultSelector: (observation, observationFilterItem) => observationFilterItem.FilterItemId)
            .Distinct();

        return await context.FilterItem
            .AsNoTracking()
            .WithSqlServerOptions("OPTION(HASH JOIN)")
            .Include(fi => fi.FilterGroup)
            .ThenInclude(fg => fg.Filter)
            .Where(fi =>
                fi.FilterGroup.Filter.SubjectId == subjectId &&
                matchedFilterItemIds.Contains(fi.Id))
            .ToListAsync();
    }

    public async Task<IList<FilterItem>> GetFilterItemsFromObservations(IEnumerable<Observation> observations)
    {
        var filterItemIds =
            observations
                .SelectMany(observation => observation.FilterItems)
                .Select(ofi => ofi.FilterItemId)
                .Distinct()
                .ToList();

        return await context
            .FilterItem
            .AsNoTracking()
            .Include(fi => fi.FilterGroup)
            .ThenInclude(fg => fg.Filter)
            .Where(fi => filterItemIds.Contains(fi.Id))
            .ToListAsync();
    }
}
