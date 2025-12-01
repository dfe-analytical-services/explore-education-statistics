#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class MatchingObservationsQueryGenerator(ITemporaryTableCreator tempTableCreator, ISqlStatementsHelper sqlHelper)
    : IMatchingObservationsQueryGenerator
{
    public async Task<(string, IList<SqlParameter>, ITempTableReference)> GetMatchingObservationsQuery(
        StatisticsDbContext context,
        Guid subjectId,
        IList<Guid> filterItemIds,
        IList<Guid> locationIds,
        TimePeriodQuery? timePeriodQuery,
        CancellationToken cancellationToken
    )
    {
        // Generate a keyless temp table to allow quick inserting of
        // matching Observation Ids into a heap in parallel.
        var matchingObservationTable = await tempTableCreator.CreateTemporaryTable<
            MatchedObservation,
            StatisticsDbContext
        >(context: context, cancellationToken: cancellationToken);

        // Generate a "WHERE" clause to limit matched Observations to selected
        // Time Periods, if any.
        var timePeriodsClause = timePeriodQuery != null ? GetTimePeriodsClause(timePeriodQuery) : default;

        // Generate a "WHERE" clause to limit matched Observations to selected
        // Locations, if any.
        var locationIdsClause =
            locationIds.Count > 0 ? await GetLocationsClause(context, locationIds, cancellationToken) : default;

        // For each Filter, generate a DELETE statement to eliminate any #MatchedObservation rows
        // that don't have any matching FilterItem selections for that Filter.
        var filterItemIdsReductions =
            filterItemIds.Count > 0
                ? await CreateDeleteStatementsForFilterItemIdSelections(
                    context: context,
                    subjectId: subjectId,
                    selectedFilterItemIds: filterItemIds,
                    matchingObservationTable: matchingObservationTable,
                    cancellationToken: cancellationToken
                )
                : default;

        // Insert matching Observation Ids into the heap.
        // Use TABLOCK to allow fast and safe parallel inserts into the table
        // and with less logging.
        // Use MAXDOP to prevent too many workers executing in parallel at once,
        // which can negatively impact performance.
        // Use RECOMPILE to tell the execution plan engine to alter its plan based
        // on fresh statistics.
        var matchingObservationSql =
            $@"
                INSERT INTO {matchingObservationTable.Name} WITH (TABLOCK)
                SELECT o.id FROM Observation o
                WHERE o.SubjectId = @subjectId "
            + (timePeriodsClause != null ? $"AND ({timePeriodsClause}) " : "")
            + (locationIdsClause != null ? $"AND ({locationIdsClause}) " : "")
            + "OPTION(RECOMPILE, MAXDOP 4);";

        // Add a unique clustered index *after* the heap insert for better performance,
        // as adding before the insert forces a more complex execution plan that orders
        // the inserts and disables parallelism.
        //
        // Update its statistics so that any future joins can make full use of its
        // accurate stats.
        var indexName = sqlHelper.CreateRandomIndexName(matchingObservationTable.Name, nameof(MatchedObservation.Id));

        var indexSql =
            $@"
                CREATE UNIQUE CLUSTERED INDEX [{indexName}]
                ON {matchingObservationTable.Name}({nameof(MatchedObservation.Id)})
                WITH (MAXDOP = 4);
            ";

        var updateStatisticsSql = $"UPDATE STATISTICS {matchingObservationTable.Name} WITH FULLSCAN;";

        SqlParameter[] parameters = [new("subjectId", subjectId)];

        return (
            @$"
            {matchingObservationSql}
            {indexSql}
            {filterItemIdsReductions ?? ""}
            {updateStatisticsSql}",
            parameters,
            matchingObservationTable
        );
    }

    /// <summary>
    /// This method generates DELETE statements to eliminate rows from #MatchedObservation that
    /// don't have matching selections of Filter Items in their rows.
    /// </summary>
    private async Task<string> CreateDeleteStatementsForFilterItemIdSelections(
        StatisticsDbContext context,
        Guid subjectId,
        IList<Guid> selectedFilterItemIds,
        ITempTableReference matchingObservationTable,
        CancellationToken cancellationToken
    )
    {
        //
        // Firstly we gather information about each Filter for this Subject and the choices the user has made.
        //
        // We find for each Filter:
        //
        // * Its ID.
        // * The Filter Items that the user has chosen.
        // * The total number of Filter Items available for the user to have chosen.
        //
        var selectedFilterItemIdsByFilter = await GetSelectedFilterItemIdsByFilter(
            context,
            selectedFilterItemIds,
            subjectId,
            cancellationToken
        );

        //
        // We are now ready to start eliminating potential candidate Observation rows from the final table results.
        // For each Filter, we are going to eliminate any candidate Observation rows that don't have a matching
        // Filter Item selected for that particular Filter e.g. if for a "School type" Filter the user selected
        // "Primary" and "Secondary", we would be eliminating any rows that had "Special" or "All schools" selected
        // in the "School type" column, for instance.
        //
        // We do this by issuing separate DELETE statements to remove candidate Observations from the
        // #MatchedObservation temp table, one for each Filter.
        //
        // We order the Filters with the most selective first - that is, the Filter with the lowest ratio of its
        // Filter Items selected first.  We do this in order to attempt to eliminate as many Observation row
        // candidates as possible in the first DELETE statement.  If for example we had a filter with 1,000 possible
        // Filter Items available, and the user selected 10 of them, they have selected only 1% of the possible options.
        // Assuming a fairly even spread of those Filter Items over, say 1,000,000 Observation rows, we will have
        // already potentially whittled our remaining candidate Observations down from 1,000,000 to 10,000.
        //
        // This means that while the first DELETE statement would have 1,000,000 Observations in its outer loop to
        // evaluate, the next would only need to consider around 10,000, and then the Filter after that many less
        // again etc.
        //
        // In contrast, had we not done this and had chosen to use a Filter first with only 2 Filter Items available
        // to it, and only one selection from the user, assuming a fairly even spread then this would only have
        // eliminated around 50% of the Observations on the first pass, leaving the next Filter to deal with
        // 500,000 candidate Observations.
        //
        var selectedFilterItemsByMostSelectiveFirst = selectedFilterItemIdsByFilter
            // Exclude any Filters for which there are no selections.
            .Where(filterItemIdsForFilter => !filterItemIdsForFilter.SelectedFilterItemIds.IsNullOrEmpty())
            // Exclude any Filters where every Filter Item has been chosen.
            .Where(filterItemIdsForFilter =>
                filterItemIdsForFilter.SelectedFilterItemIds.Count < filterItemIdsForFilter.TotalFilterItemCount
            )
            // Order any remaining Filters by how selective they will be
            // (how likely they will be to exclude as many Observations as possible)
            .OrderByDescending(filterItemIdsForFilter => filterItemIdsForFilter.SelectivenessFactor);

        // For each Filter, create its own temp table with its respective Filter Item Ids in it.
        // This can either be the selected Filter Items for the Filter or the *unselected* ones,
        // depending on which is the most efficient to use.
        var filterItemIdTempTablesPerFilter = selectedFilterItemsByMostSelectiveFirst.ToDictionary(
            filterItemIdsForFilter => filterItemIdsForFilter,
            filterItemIdsForFilter =>
            {
                var filterItemIdsForExclusionClause = filterItemIdsForFilter
                    .FilterItemIdsToUseInCheck.OrderBy(id => id)
                    .Select(id => new IdTempTable(id))
                    .ToList();

                return tempTableCreator
                    .CreateAndPopulateTemporaryTable(context, filterItemIdsForExclusionClause, cancellationToken)
                    .Result;
            }
        );

        // For each Filter, issue a DELETE statement to exclude Observations if
        // they don't meet the criteria of the Filter Item selection.
        var deleteStatements = filterItemIdTempTablesPerFilter.Select(filterItemIdTempTableForFilter =>
        {
            var exclusionType = filterItemIdTempTableForFilter.Key.ExclusionType;
            var filterItemIdsTempTableName = filterItemIdTempTableForFilter.Value.Name;

            return @$"
            DELETE CandidateObservation WITH (TABLOCK)
            FROM {matchingObservationTable.Name} CandidateObservation
            WHERE {(exclusionType == FilterAndFilterItemInfo.ExclusionCheckType.NotExists ? "NOT" : "")} EXISTS (
                SELECT 1
                FROM ObservationFilterItem OFI
                JOIN {filterItemIdsTempTableName} SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                WHERE OFI.ObservationId = CandidateObservation.Id
            )
            OPTION(RECOMPILE, MAXDOP 4);
            ";
        });

        return deleteStatements.JoinToString("\n\n");
    }

    private static async Task<List<FilterAndFilterItemInfo>> GetSelectedFilterItemIdsByFilter(
        StatisticsDbContext context,
        IList<Guid> filterItemIds,
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        var filtersForSubject = await context
            .Filter.Include(filter => filter.FilterGroups)
                .ThenInclude(filterGroup => filterGroup.FilterItems)
            .Where(filterItem => filterItem.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        return filtersForSubject
            .Select(filter =>
            {
                var allFilterItemIdsForFilter = filter
                    .FilterGroups.SelectMany(f => f.FilterItems)
                    .Select(f => f.Id)
                    .ToList();
                var selectedFilterItemsForFilter = allFilterItemIdsForFilter.Intersect(filterItemIds).ToList();
                return new FilterAndFilterItemInfo
                {
                    SelectedFilterItemIds = selectedFilterItemsForFilter,
                    UnselectedFilterItemIds = allFilterItemIdsForFilter.Except(selectedFilterItemsForFilter).ToList(),
                };
            })
            .ToList();
    }

    private async Task<string> GetLocationsClause(
        StatisticsDbContext context,
        IList<Guid> locationIds,
        CancellationToken cancellationToken = default
    )
    {
        var locationsTempTable = await tempTableCreator.CreateAndPopulateTemporaryTable(
            context,
            locationIds.Select(id => new IdTempTable(id)),
            cancellationToken
        );

        return $"o.LocationId IN (SELECT Id FROM {locationsTempTable.Name})";
    }

    private static string GetTimePeriodsClause(TimePeriodQuery timePeriodQuery)
    {
        var timePeriods = TimePeriodUtil.Range(timePeriodQuery).ToList();
        var timePeriodClauses = timePeriods.Select(timePeriod =>
            $"(o.TimeIdentifier = '{timePeriod.TimeIdentifier.GetEnumValue()}' AND o.Year = {timePeriod.Year})"
        );

        return timePeriodQuery.Limit is not null
            ? timePeriodClauses.Take(timePeriodQuery.Limit.Value).JoinToString(" OR ")
            : timePeriodClauses.JoinToString(" OR ");
    }

    // ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Local
    private record FilterAndFilterItemInfo
    {
        public enum ExclusionCheckType
        {
            Exists,
            NotExists,
        }

        public required List<Guid> SelectedFilterItemIds { get; init; }

        public required List<Guid> UnselectedFilterItemIds { get; init; }

        public int TotalFilterItemCount => SelectedFilterItemIds.Count + UnselectedFilterItemIds.Count;

        public ExclusionCheckType ExclusionType =>
            SelectedFilterItemIds.Count > UnselectedFilterItemIds.Count
                ? ExclusionCheckType.Exists
                : ExclusionCheckType.NotExists;

        public List<Guid> FilterItemIdsToUseInCheck =>
            ExclusionType == ExclusionCheckType.Exists ? UnselectedFilterItemIds : SelectedFilterItemIds;

        public double SelectivenessFactor => (double)TotalFilterItemCount / SelectedFilterItemIds.Count;
    }
}
