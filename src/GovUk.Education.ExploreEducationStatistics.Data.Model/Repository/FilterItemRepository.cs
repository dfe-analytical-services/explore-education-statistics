#nullable enable
using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class FilterItemRepository(
    StatisticsDbContext context,
    ILogger<FilterItemRepository> logger) : IFilterItemRepository
{
    public IMatchingFilterItemsQueryGenerator QueryGenerator =
        new MatchingFilterItemsQueryGenerator();

    public IRawSqlExecutor SqlExecutor = new RawSqlExecutor();


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

    public interface IMatchingFilterItemsQueryGenerator
    {
        Task<(string, IList<IAsyncDisposable>)> GetMatchingFilterItemsQuery(
            StatisticsDbContext context,
            CancellationToken cancellationToken);
    }

    public class MatchingFilterItemsQueryGenerator : IMatchingFilterItemsQueryGenerator
    {
        public ITemporaryTableCreator TempTableCreator = new TemporaryTableCreator();

        public async Task<(string, IList<IAsyncDisposable>)> GetMatchingFilterItemsQuery(
            StatisticsDbContext context,
            CancellationToken cancellationToken)
        {
            // Generate a keyless temp table to allow quick inserting of
            // matching FilterItems Ids into a heap in parallel.
            var matchedFilterItemsTempTable =
                await TempTableCreator.CreateTemporaryTable<MatchedFilterItem, StatisticsDbContext>(
                    context: context,
                    cancellationToken: cancellationToken);

            // Insert matching FilterItem Ids into the heap.
            // Use TABLOCK to allow fast and safe parallel inserts into the table
            // and with less logging.
            // Use MAXDOP to prevent too many workers executing in parallel at once,
            // which can negatively impact performance.
            // Use RECOMPILE to tell the execution plan engine to alter its plan based
            // on fresh statistics.
            var matchingFilterItemsSql = $@"
                INSERT INTO {matchedFilterItemsTempTable.Name} WITH (TABLOCK)
                SELECT DISTINCT o.FilterItemId
                FROM #{nameof(MatchedObservation)} AS mo
                JOIN ObservationFilterItem AS o
                  ON o.ObservationId = mo.Id
                OPTION(RECOMPILE, MAXDOP 4)";

            // Add a unique clustered index *after* the heap insert for better performance,
            // as adding before the insert forces a more complex execution plan that orders
            // the inserts and disables parallelism.
            var indexSql = $@"
                CREATE UNIQUE CLUSTERED INDEX [IX_MatchedFilterItem_FilterItemId_{Guid.NewGuid()}]
                ON #MatchedFilterItem(Id) WITH (MAXDOP = 4);";

            return ($"{matchingFilterItemsSql}\n\n{indexSql}", [matchedFilterItemsTempTable]);
        }
    }

    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        IQueryable<MatchedObservation> matchedObservations,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var (sql, tempTables) = await QueryGenerator.GetMatchingFilterItemsQuery(
            context: context,
            cancellationToken: cancellationToken);

        try
        {
            // Execute the query to find matching FilterItem Ids.
            await SqlExecutor.ExecuteSqlRaw(
                sql: sql,
                context: context,
                parameters: [],
                cancellationToken: cancellationToken);
            
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace(
                    "Finished finding matching FilterItems in a total of {Milliseconds} ms",
                    sw.Elapsed.TotalMilliseconds);
            }
            
            sw.Restart();

            // Using the Ids of the matching FilterItems, fetch the details of the FilterItems,
            // their FilterGroups and Filters.
            var filterItems = await context
                .FilterItem
                .AsNoTracking()
                .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Join(
                    inner: context.MatchedFilterItems,
                    outerKeySelector: filterItem => filterItem.Id,
                    innerKeySelector: matchedFilterItem => matchedFilterItem.Id,
                    resultSelector: (filterItem, matchedFilterItem) => filterItem)
                .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
                .WithSqlServerOptions("OPTION(RECOMPILE)")
                .ToListAsync(cancellationToken);
            
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace(
                    "Finished fetching {FilterItemCount} FilterItems and their " +
                    "FilterGroups / Filters in a total of {Milliseconds} ms",
                    filterItems.Count,
                    sw.Elapsed.TotalMilliseconds);
            }
            
            return filterItems;
        }
        finally
        {
            // Although EF and SQL Server will clean temporary tables up eventually themselves when the Controller
            // method finishes (and thus the DB connection is disposed), it's nice to leave things as cleared down
            // as possible before exiting this method.
            await tempTables
                .ToAsyncEnumerable()
                // ReSharper disable once MethodSupportsCancellation - don't want to cancel the cleaning up of 
                // temporary tables.
                .ForEachAwaitAsync(async tempTable => await tempTable.DisposeAsync());
        }
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
