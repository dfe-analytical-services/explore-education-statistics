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
/// a high percentage of Observation rows matched so far from a Subject's available
/// Observations.
///
/// For each FilterItemId, we scan down its ObservationFilterItem ObservationIds
/// (using the IX_ObservationFilterItem_FilterItemId_ObservationId index), and for
/// each ObservationId, we check for the existence of a matching ObservationId in the
/// #MatchedObservation temp table. As soon as we find a hit, we move on to the next
/// FilterItem in the list, and if we don't find a matching ObservationId, then this
/// FilterItem is not a candidate for the "Choose filters" step.
///
/// The reason that this strategy works well is that:
///
/// 1) The FilterItem set (which is the driving set for this query) is generally a much
/// smaller set than the Subject's Observation rows or ObservationFilterItem rows, so
/// will create much fewer top-level loops.
///
/// 2) The fact that we choose this strategy only when there are a high percentage of
/// ObservationIds in the #MatchedObservation means that generally we quickly find a
/// matching ObservationId in #MatchedObservations for each FilterItem (i.e. there's a
/// much higher chance to find a match quickly if there are lots of Ids to match against),
/// meaning that we don't have to do much work for the most part to find matches for each
/// FilterItem and thus can break early.  As the percentage drops, the duration of this
/// query increases dramatically, which is when we switch to using
/// <see cref="SparseObservationsMatchedFilterItemsStrategy"/> instead.
/// </summary>
public class DenseObservationsMatchedFilterItemsStrategy(
    IRawSqlExecutor sqlExecutor,
    ITemporaryTableCreator temporaryTableCreator,
    ISqlStatementsHelper sqlHelper,
    StatisticsDbContext context,
    ILogger<DenseObservationsMatchedFilterItemsStrategy> logger
) : IDenseObservationsMatchedFilterItemsStrategy
{
    public async Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
        Guid subjectId,
        ITempTableReference matchedObservationsTableReference,
        CancellationToken cancellationToken
    )
    {
        var sw = Stopwatch.StartNew();

        var allFilterItemIds = await context
            .FilterItem.Where(fi => fi.FilterGroup.Filter.SubjectId == subjectId)
            .Select(fi => fi.Id)
            .ToListAsync(cancellationToken);

        var candidateFilterItemIdTable = await temporaryTableCreator.CreateAndPopulateTemporaryTable(
            context,
            allFilterItemIds.Select(fi => new IdTempTable(fi)),
            cancellationToken
        );

        var matchedFilterItemIdTable = await temporaryTableCreator.CreateTemporaryTable<
            MatchedFilterItem,
            StatisticsDbContext
        >(context, cancellationToken);

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
            SELECT CandidateFilterItem.Id
            FROM {candidateFilterItemIdTable.Name} AS CandidateFilterItem
            WHERE EXISTS
            (
                SELECT 1
                FROM dbo.ObservationFilterItem AS OFI
                JOIN {matchedObservationsTableReference.Name} AS Observation
                    ON Observation.Id = ofi.ObservationId
                WHERE OFI.FilterItemId = CandidateFilterItem.Id
            )
            OPTION (RECOMPILE, MAXDOP 4);";

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
