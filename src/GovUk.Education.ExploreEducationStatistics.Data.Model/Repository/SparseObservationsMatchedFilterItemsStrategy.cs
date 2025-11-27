using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

/// <summary>
/// The purpose of this strategy is to find candidate Filter Items for the table
/// tool's "Choose filters" step, based on the Observations that have been matched
/// so far from Location and Time Period choices.
///
/// This strategy is optimised to find candidate FilterItems based on there being
/// a lower percentage of Observation rows matched so far from a Subject's available
/// Observations.
///
/// In this strategy, we use #MatchedObservations as the driving set for the query,
/// because it has a lower percentage of a Subject's Observations matched, and is thus
/// a smaller set.
///
/// For each ObservationId, we join the ObservationFilterItem table and find all
/// distinct FilterItemIds that appear for that ObservationId, using the
/// IX_ObservationFilterItem_FilterItemId_ObservationId index for extra speed.
/// The query executor will generally pick a good strategy for applying this, making
/// use of parallelism to break the job into batches of ObservationIds and eventually
/// combining and de-duping the FilterItemIds found.
///
/// The reason that this strategy works well is that the #MatchedObservation temp table
/// (which is the driving set for this query) is a smaller set because we only use this
/// strategy at lower percentages of matched Observations.  The query executor can also
/// make use of parallelism to execute the job quicker.
/// </summary>
public class SparseObservationsMatchedFilterItemsStrategy(
    IRawSqlExecutor sqlExecutor,
    ITemporaryTableCreator temporaryTableCreator,
    ISqlStatementsHelper sqlHelper,
    StatisticsDbContext context,
    ILogger<SparseObservationsMatchedFilterItemsStrategy> logger
) : ISparseObservationsMatchedFilterItemsStrategy
{
    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    )
    {
        var sw = Stopwatch.StartNew();

        // Generate a keyless temp table to allow quick inserting of
        // matching FilterItems Ids into a heap in parallel.
        var matchedFilterItemIdTable = await temporaryTableCreator.CreateTemporaryTable<
            MatchedFilterItem,
            StatisticsDbContext
        >(context: context, cancellationToken: cancellationToken);

        // Insert matching FilterItem Ids into the heap.
        // Use TABLOCK to allow fast and safe parallel inserts into the table
        // and with less logging.
        // Use MAXDOP to prevent too many workers executing in parallel at once,
        // which can negatively impact performance.
        // Use RECOMPILE to tell the execution plan engine to alter its plan based
        // on fresh statistics.
        var matchedFilterItemsSql =
            $@"
                INSERT INTO {matchedFilterItemIdTable.Name} WITH (TABLOCK)
                SELECT DISTINCT o.FilterItemId
                FROM {matchedObservationsTableReference.Name} AS mo
                JOIN ObservationFilterItem AS o
                  ON o.ObservationId = mo.Id
                OPTION(RECOMPILE, MAXDOP 4);";

        // Execute the query to find matching FilterItem Ids and insert them into
        // the #MatchedFilterItem temp table.
        await sqlExecutor.ExecuteSqlRaw(
            sql: matchedFilterItemsSql,
            context: context,
            cancellationToken: cancellationToken
        );

        // Add a unique clustered index *after* the heap insert for better performance,
        // as adding before the insert forces a more complex execution plan that orders
        // the inserts and disables parallelism.
        //
        // Update its statistics so that any future joins can make full use of its
        // accurate stats.
        var indexName = sqlHelper.CreateRandomIndexName(matchedFilterItemIdTable.Name, nameof(MatchedFilterItem.Id));

        var indexSql =
            $@"
            CREATE UNIQUE CLUSTERED INDEX [{indexName}]
            ON {matchedFilterItemIdTable.Name}({nameof(MatchedFilterItem.Id)}) WITH (MAXDOP = 4);

            UPDATE STATISTICS {matchedFilterItemIdTable.Name} WITH FULLSCAN;";

        // Execute the query to add the index to the temp table.
        await sqlExecutor.ExecuteSqlRaw(sql: indexSql, context: context, cancellationToken: cancellationToken);

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Finished finding matching FilterItems in a total of {Milliseconds} ms",
                sw.Elapsed.TotalMilliseconds
            );
        }

        sw.Restart();

        // Using the Ids of the matching FilterItems, fetch the details of the FilterItems,
        // their FilterGroups and Filters.
        var filterItems = await context
            .FilterItem.AsNoTracking()
            .Include(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
            .Join(
                // This is the contents of the #MatchedFilterItem temp table.
                inner: context.MatchedFilterItems,
                outerKeySelector: filterItem => filterItem.Id,
                innerKeySelector: matchedFilterItem => matchedFilterItem.Id,
                resultSelector: (filterItem, matchedFilterItem) => filterItem
            )
            .Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
            .WithSqlServerOptions("OPTION(RECOMPILE)")
            .ToListAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Finished fetching {FilterItemCount} FilterItems and their "
                    + "FilterGroups / Filters in a total of {Milliseconds} ms",
                filterItems.Count,
                sw.Elapsed.TotalMilliseconds
            );
        }

        return filterItems;
    }
}
