#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class FilterItemRepository(
    StatisticsDbContext statisticsDbContext,
    IAllObservationsMatchedFilterItemsStrategy allObservationsMatchedFilterItemsStrategy,
    ISparseObservationsMatchedFilterItemsStrategy sparseMatchedFilterItemStrategy,
    IDenseObservationsMatchedFilterItemsStrategy denseMatchedFilterItemStrategy,
    ILogger<FilterItemRepository> logger) : IFilterItemRepository
{
    private const int PercentageObservationsFoundToUseDenseStrategy = 75;
    
    public async Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds)
    {
        var filterItems = await statisticsDbContext.FilterItem
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
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken)
    {
        var matchedObservationCount = await statisticsDbContext
            .MatchedObservations
            .CountAsync(cancellationToken);
        
        var fullObservationCount = await statisticsDbContext
            .Observation
            .CountAsync(o => o.SubjectId == subjectId, cancellationToken);

        var percentageObservationsFound = matchedObservationCount * 100 / fullObservationCount;

        logger.LogDebug(
            message: "Found {PercantageObservationsFound}% Observations so far.",
            percentageObservationsFound);
        
        // Based on the number of Observations matched so far versus the number of
        // Observations actually on the Subject itself, choose the best strategy
        // for finding the matching Filter Items.
        if (percentageObservationsFound == 100)
        {
            logger.LogDebug(
                message: "Using {Strategy} to find FilterItems.",
                nameof(allObservationsMatchedFilterItemsStrategy));
            
            return await allObservationsMatchedFilterItemsStrategy
                .GetFilterItemsFromMatchedObservationIds(
                    subjectId,
                    cancellationToken);
        }

        // If we've matched 80% or more Observations of the Subject, favour the approach
        // that matches Filter Items quickest against a large set of Observations.
        if (percentageObservationsFound >= PercentageObservationsFoundToUseDenseStrategy)
        {
            logger.LogDebug(
                message: "Using {Strategy} to find FilterItems.",
                nameof(denseMatchedFilterItemStrategy));

            return await denseMatchedFilterItemStrategy
                .GetFilterItemsFromMatchedObservationIds(
                    subjectId: subjectId,
                    matchedObservationsTableReference: matchedObservationsTableReference,
                    cancellationToken: cancellationToken);
        }
        
        logger.LogDebug(
            message: "Using {Strategy} to find FilterItems.",
            nameof(sparseMatchedFilterItemStrategy));
        
        // If we've matched less than 80% of the Observations of the Subject, favour the approach
        // that matches Filter Items quickest against a smaller set of Observations.
        return await sparseMatchedFilterItemStrategy
            .GetFilterItemsFromMatchedObservationIds(
                subjectId: subjectId,
                matchedObservationsTableReference: matchedObservationsTableReference,
                cancellationToken: cancellationToken);
    }

    public async Task<IList<FilterItem>> GetFilterItemsFromObservations(IEnumerable<Observation> observations)
    {
        var filterItemIds =
            observations
                .SelectMany(observation => observation.FilterItems)
                .Select(ofi => ofi.FilterItemId)
                .Distinct()
                .ToList();

        return await statisticsDbContext
            .FilterItem
            .AsNoTracking()
            .Include(fi => fi.FilterGroup)
            .ThenInclude(fg => fg.Filter)
            .Where(fi => filterItemIds.Contains(fi.Id))
            .ToListAsync();
    }
}
