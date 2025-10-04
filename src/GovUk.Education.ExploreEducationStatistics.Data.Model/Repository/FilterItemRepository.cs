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
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class FilterItemRepository(
    StatisticsDbContext statisticsDbContext,
    SparseObservationsMatchedFilterItemsStrategy sparseMatchedFilterItemStrategy,
    DenseObservationsMatchedFilterItemsStrategy denseMatchedFilterItemStrategy
) : IFilterItemRepository
{
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

        // As a quick optimisation, if ALL Observations have been matched so far,
        // then all Filter Items are included in the results so far.
        if (matchedObservationCount == fullObservationCount)
        {
            return await statisticsDbContext
                .FilterItem
                .AsNoTracking()
                .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
                .ToListAsync(cancellationToken);
        }

        if (matchedObservationCount >= fullObservationCount * 0.8)
        {
            return await denseMatchedFilterItemStrategy
                .GetFilterItemsFromMatchedObservationIds(
                    subjectId: subjectId,
                    matchedObservationsTableReference: matchedObservationsTableReference,
                    cancellationToken: cancellationToken);
        }
        
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

public interface IMatchedFilterItemsStrategy
{
    Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken);
}

public class SparseObservationsMatchedFilterItemsStrategy(
    StatisticsDbContext context,
    ILogger<SparseObservationsMatchedFilterItemsStrategy> logger)
    : IMatchedFilterItemsStrategy
{
    public IMatchedFilterItemsQueryGenerator QueryGenerator =
        new MatchedFilterItemsQueryGenerator();

    public IRawSqlExecutor SqlExecutor = new RawSqlExecutor();

    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var sql = await QueryGenerator.GetMatchedFilterItemsQuery(
            context: context,
            context.MatchedObservations,
            matchedObservationsTableReference,
            cancellationToken: cancellationToken);

        // Execute the query to find matching FilterItem Ids and insert them into
        // the #MatchedFilterItem temp table.
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
                // This is the contents of the #MatchedFilterItem temp table.
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

    public interface IMatchedFilterItemsQueryGenerator
    {
        Task<string> GetMatchedFilterItemsQuery(
            StatisticsDbContext context,
            IQueryable<MatchedObservation> matchedObservations,
            ITempTableReference matchedObservationsTableReference,
            CancellationToken cancellationToken);
    }

    public class MatchedFilterItemsQueryGenerator : IMatchedFilterItemsQueryGenerator
    {
        public ITemporaryTableCreator TempTableCreator = new TemporaryTableCreator();

        public async Task<string> GetMatchedFilterItemsQuery(
            StatisticsDbContext context,
            IQueryable<MatchedObservation> _,
            ITempTableReference matchedObservationsTableReference,
            CancellationToken cancellationToken)
        {
            // Generate a keyless temp table to allow quick inserting of
            // matching FilterItems Ids into a heap in parallel.
            var matchedFilterItemIdTable =
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
            var matchedFilterItemsSql = $@"
                INSERT INTO {matchedFilterItemIdTable.Name} WITH (TABLOCK)
                SELECT DISTINCT o.FilterItemId
                FROM {matchedObservationsTableReference.Name} AS mo
                JOIN ObservationFilterItem AS o
                  ON o.ObservationId = mo.Id
                OPTION(RECOMPILE, MAXDOP 4);";

            // Add a unique clustered index *after* the heap insert for better performance,
            // as adding before the insert forces a more complex execution plan that orders
            // the inserts and disables parallelism.
            var indexSql = $@"
                CREATE UNIQUE CLUSTERED INDEX [IX_{matchedFilterItemIdTable.Name}_{nameof(MatchedFilterItem.Id)}_{Guid.NewGuid()}]
                ON {matchedFilterItemIdTable.Name}({nameof(MatchedFilterItem.Id)}) WITH (MAXDOP = 4);";

            return $"{matchedFilterItemsSql}\n\n{indexSql}";
        }
    }
}

public class DenseObservationsMatchedFilterItemsStrategy(
    StatisticsDbContext context,
    ILogger<DenseObservationsMatchedFilterItemsStrategy> logger)
    : IMatchedFilterItemsStrategy
{
    public IMatchedFilterItemsQueryGenerator QueryGenerator =
        new MatchedFilterItemsQueryGenerator();

    public IRawSqlExecutor SqlExecutor = new RawSqlExecutor();

    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var sql = await QueryGenerator.GetMatchedFilterItemsQuery(
            context: context,
            subjectId,
            context.MatchedObservations,
            matchedObservationsTableReference,
            cancellationToken: cancellationToken);

        // Execute the query to find matching FilterItem Ids and insert them into
        // the #MatchedFilterItem temp table.
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
                // This is the contents of the #MatchedFilterItem temp table.
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

    public interface IMatchedFilterItemsQueryGenerator
    {
        Task<string> GetMatchedFilterItemsQuery(
            StatisticsDbContext context,
            Guid subjectId,
            IQueryable<MatchedObservation> matchedObservations,
            ITempTableReference matchedObservationsTableReference,
            CancellationToken cancellationToken);
    }

    public class MatchedFilterItemsQueryGenerator : IMatchedFilterItemsQueryGenerator
    {
        public ITemporaryTableCreator TempTableCreator = new TemporaryTableCreator();

        public async Task<string> GetMatchedFilterItemsQuery(
            StatisticsDbContext context,
            Guid subjectId,
            IQueryable<MatchedObservation> _,
            ITempTableReference matchedObservationsTableReference,
            CancellationToken cancellationToken)
        {
            var allFilterItemIds = await context
                .FilterItem
                .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
                .Select(fi => fi.Id)
                .ToListAsync(cancellationToken);
            
            var candidateFilterItemIdTable = await TempTableCreator
                .CreateAnonymousTemporaryTableAndPopulate(
                    context,
                    allFilterItemIds.Select(fi => new IdTempTable(fi)),
                    cancellationToken);
            
            var matchedFilterItemIdTable = await TempTableCreator
                .CreateTemporaryTable<MatchedFilterItem, StatisticsDbContext>(context, cancellationToken);
            
            // TODO check below notes are right!
            // Insert matching FilterItem Ids into the heap.
            // Use TABLOCK to allow fast and safe parallel inserts into the table
            // and with less logging.
            // Use MAXDOP to prevent too many workers executing in parallel at once,
            // which can negatively impact performance.
            // Use RECOMPILE to tell the execution plan engine to alter its plan based
            // on fresh statistics.
            var matchedFilterItemsSql = $@"
                INSERT INTO {matchedFilterItemIdTable.Name} WITH (TABLOCK)
                SELECT c.Id
                FROM {candidateFilterItemIdTable.Name} AS c
                CROSS APPLY (
                    SELECT TOP (1) 1 AS HasMatch
                    FROM dbo.ObservationFilterItem AS ofi --WITH (FORCESEEK, INDEX(IX_OFI_Filter_Obs))
                    JOIN {matchedObservationsTableReference.Name} AS m WITH (INDEX(1))
                      ON m.Id = ofi.ObservationId
                    WHERE ofi.FilterItemId = c.Id
                ) AS hit
                OPTION (RECOMPILE, LOOP JOIN);"; // , MAXDOP 4?

            // Add a unique clustered index *after* the heap insert for better performance,
            // as adding before the insert forces a more complex execution plan that orders
            // the inserts and disables parallelism.
            var indexSql = $@"
                CREATE UNIQUE CLUSTERED INDEX [IX_{matchedFilterItemIdTable.Name}_{nameof(MatchedFilterItem.Id)}_{Guid.NewGuid()}]
                ON {matchedFilterItemIdTable.Name}({nameof(MatchedFilterItem.Id)}) WITH (MAXDOP = 4);";

            return $"{matchedFilterItemsSql}\n\n{indexSql}";
        }
    }
}
