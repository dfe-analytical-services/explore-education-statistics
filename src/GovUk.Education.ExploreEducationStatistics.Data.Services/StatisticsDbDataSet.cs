#nullable enable
using System.Runtime.CompilerServices;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class StatisticsDbDataSet(
    Guid subjectId,
    StatisticsDbContext context,
    IObservationService observationService,
    IFilterRepository filterRepository,
    IIndicatorGroupRepository indicatorGroupRepository,
    ILocationRepository locationRepository,
    IAllObservationsMatchedFilterItemsStrategy allObservationsMatchedFilterItemsStrategy,
    ISparseObservationsMatchedFilterItemsStrategy sparseObservationsMatchedFilterItemsStrategy,
    IDenseObservationsMatchedFilterItemsStrategy denseObservationsMatchedFilterItemsStrategy,
    ILogger<StatisticsDbDataSet> logger
) : IStorageDataSet
{
    private const int PercentageObservationsFoundToUseDenseStrategy = 75;

    public Guid SubjectId { get; } = subjectId;

    public async Task<List<Observation>> ListObservations(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var observations = await BuildMatchedObservationsQuery(query, cancellationToken);
        return await observations.ToListAsync(cancellationToken);
    }

    public async IAsyncEnumerable<IReadOnlyList<Observation>> ListObservationBatches(
        FullTableQuery query,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var observations = await BuildMatchedObservationsQuery(query, cancellationToken);

        var batch = new List<Observation>(batchSize);

        await foreach (var observation in observations.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            batch.Add(observation);

            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<Observation>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<List<FilterItem>> ListFilterItemsForQuery(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ValidateQuery(query);

        var matchedObservationsTableReference = await observationService.GetMatchedObservations(
            query,
            cancellationToken
        );

        return await ListFilterItemsFromMatchedObservations(matchedObservationsTableReference, cancellationToken);
    }

    public async Task<List<FilterItem>> ListFilterItems(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    )
    {
        var filterItemIdList = filterItemIds.ToList();

        return await context
            .FilterItem.AsNoTracking()
            .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
            .Where(fi => filterItemIdList.Contains(fi.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> CountFilterItemsByFilter(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    )
    {
        var filterItemIdList = filterItemIds.ToList();

        var filterItems = await context
            .FilterItem.Include(filterItem => filterItem.FilterGroup)
            .Where(filterItem => filterItemIdList.Contains(filterItem.Id))
            .ToListAsync(cancellationToken);

        var notFound = filterItemIdList
            .Where(id => filterItems.All(found => found.Id != id))
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

    public Task<List<Filter>> ListFilters(CancellationToken cancellationToken = default)
    {
        return filterRepository.GetFiltersIncludingItems(SubjectId);
    }

    public async Task<List<Indicator>> ListIndicators(CancellationToken cancellationToken = default)
    {
        return await context
            .Indicator.AsNoTracking()
            .Where(indicator => indicator.IndicatorGroup.SubjectId == SubjectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Indicator>> ListIndicators(
        IEnumerable<Guid> indicatorIds,
        CancellationToken cancellationToken = default
    )
    {
        var indicatorIdList = indicatorIds.ToList();

        return await context
            .Indicator.AsNoTracking()
            .Where(indicator => indicator.IndicatorGroup.SubjectId == SubjectId)
            .Where(indicator => indicatorIdList.Contains(indicator.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default)
    {
        return indicatorGroupRepository.GetIndicatorGroups(SubjectId);
    }

    public async Task<List<Location>> ListLocations(CancellationToken cancellationToken = default)
    {
        return [.. await locationRepository.GetDistinctForSubject(SubjectId)];
    }

    public async Task<List<Location>> ListLocations(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    )
    {
        var locationIdList = locationIds.ToList();

        return await context
            .Location.AsNoTracking()
            .Where(location => locationIdList.Contains(location.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        CancellationToken cancellationToken = default
    )
    {
        var observations = context.Observation.AsNoTracking().Where(o => o.SubjectId == SubjectId);

        return TimePeriodQueryUtils.ListDistinctTimePeriods(observations, cancellationToken);
    }

    public Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    )
    {
        var locationIdList = locationIds.ToList();

        var observations = context
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == SubjectId && EF.Constant(locationIdList).Contains(o.LocationId));

        return TimePeriodQueryUtils.ListDistinctTimePeriods(observations, cancellationToken);
    }

    /// <summary>
    /// Populates the #MatchedObservation temporary table with the ids of the observations matching the query and
    /// returns a queryable over the corresponding observations. The query is not executed here, allowing callers
    /// to either materialise it in full or stream it.
    /// </summary>
    private async Task<IQueryable<Observation>> BuildMatchedObservationsQuery(
        FullTableQuery query,
        CancellationToken cancellationToken
    )
    {
        ValidateQuery(query);

        await observationService.GetMatchedObservations(query, cancellationToken);

        var matchedObservationIds = context.MatchedObservations.Select(o => o.Id);

        return context
            .Observation.AsNoTracking()
            .Include(o => o.Location)
            .Include(o => o.FilterItems)
            .Where(o => matchedObservationIds.Contains(o.Id));
    }

    /// <summary>
    /// Retrieves the filter items present on the observations whose ids have already been stored in the
    /// #MatchedObservation temporary table, choosing the best strategy based on how many observations were
    /// matched relative to the total number of observations for the data set.
    /// </summary>
    private async Task<List<FilterItem>> ListFilterItemsFromMatchedObservations(
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    )
    {
        var matchedObservationCount = await context.MatchedObservations.CountAsync(cancellationToken);

        // If no Observations have been matched, simply return no Filter Items.
        if (matchedObservationCount == 0)
        {
            logger.LogDebug(message: "No Observations matched. Returning no Filter Items.");
            return [];
        }

        var fullObservationCount = await context.Observation.CountAsync(
            o => o.SubjectId == SubjectId,
            cancellationToken
        );

        // If all Observations have been matched so far, return all Filter Items.
        if (matchedObservationCount == fullObservationCount)
        {
            logger.LogDebug(
                message: "Using {Strategy} to find FilterItems.",
                nameof(allObservationsMatchedFilterItemsStrategy)
            );

            var allFilterItems =
                await allObservationsMatchedFilterItemsStrategy.GetFilterItemsFromMatchedObservationIds(
                    SubjectId,
                    cancellationToken
                );

            return [.. allFilterItems];
        }

        var percentageObservationsFound = matchedObservationCount * 100 / fullObservationCount;

        logger.LogDebug(
            message: "Found {PercentageObservationsFound}% Observations so far.",
            percentageObservationsFound
        );

        // If we've matched a particular percentage threshold or more Observations so far,
        // favour the approach that matches Filter Items quickest against a large set of
        // Observations.
        if (percentageObservationsFound >= PercentageObservationsFoundToUseDenseStrategy)
        {
            logger.LogDebug(
                message: "Using {Strategy} to find FilterItems.",
                nameof(denseObservationsMatchedFilterItemsStrategy)
            );

            var denseFilterItems =
                await denseObservationsMatchedFilterItemsStrategy.GetFilterItemsFromMatchedObservationIds(
                    subjectId: SubjectId,
                    matchedObservationsTableReference: matchedObservationsTableReference,
                    cancellationToken: cancellationToken
                );

            return [.. denseFilterItems];
        }

        logger.LogDebug(
            message: "Using {Strategy} to find FilterItems.",
            nameof(sparseObservationsMatchedFilterItemsStrategy)
        );

        // If we've matched less than the percentage threshold of the Observations so far,
        // favour the approach that matches Filter Items quickest against a smaller set of
        // Observations.
        var sparseFilterItems =
            await sparseObservationsMatchedFilterItemsStrategy.GetFilterItemsFromMatchedObservationIds(
                subjectId: SubjectId,
                matchedObservationsTableReference: matchedObservationsTableReference,
                cancellationToken: cancellationToken
            );

        return [.. sparseFilterItems];
    }

    private void ValidateQuery(FullTableQuery query)
    {
        if (query.SubjectId != SubjectId)
        {
            throw new ArgumentException(
                $"Query SubjectId {query.SubjectId} does not match data set SubjectId {SubjectId}",
                nameof(query)
            );
        }
    }
}
