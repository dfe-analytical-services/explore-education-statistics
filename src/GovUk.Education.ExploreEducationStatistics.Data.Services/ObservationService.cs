#nullable enable
using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.EntityFrameworkCore.TempTables;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class ObservationService : IObservationService
{
    private readonly StatisticsDbContext _context;
    private readonly ILogger<ObservationService> _logger;

    public IMatchingObservationsQueryGenerator QueryGenerator = new MatchingObservationsQueryGenerator();

    public IRawSqlExecutor SqlExecutor = new RawSqlExecutor();

    public ObservationService(StatisticsDbContext context,
        ILogger<ObservationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ITempTableReference> GetMatchedObservations(
        FullTableQuery query,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var (sql, sqlParameters, matchingObservationTable) = await QueryGenerator
            .GetMatchingObservationsQuery(
                _context,
                query.SubjectId,
                query.GetFilterItemIds(),
                query.LocationIds,
                query.TimePeriod,
                cancellationToken);

        // Execute the query to find matching Observation Ids.
        await SqlExecutor.ExecuteSqlRaw(
            context: _context,
            sql: sql,
            parameters: sqlParameters,
            cancellationToken: cancellationToken);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Finished fetching {ObservationCount} Observations in a total of {Milliseconds} ms",
                _context.MatchedObservations.Count(),
                sw.Elapsed.TotalMilliseconds);
        }

        return matchingObservationTable;
    }
    
    public interface IMatchingObservationsQueryGenerator
    {
        Task<(string, IList<SqlParameter>, ITempTableReference)> GetMatchingObservationsQuery(
            StatisticsDbContext context,
            Guid subjectId,
            IList<Guid> filterItemIds,
            IList<Guid> locationIds,
            TimePeriodQuery? timePeriodQuery,
            CancellationToken cancellationToken);
    }

    public class MatchingObservationsQueryGenerator : IMatchingObservationsQueryGenerator
    {
        public ITemporaryTableCreator TempTableCreator = new TemporaryTableCreator();

        public async Task<(string, IList<SqlParameter>, ITempTableReference)>
            GetMatchingObservationsQuery(
            StatisticsDbContext context,
            Guid subjectId,
            IList<Guid> filterItemIds,
            IList<Guid> locationIds,
            TimePeriodQuery? timePeriodQuery,
            CancellationToken cancellationToken)
        {
            // Generate a keyless temp table to allow quick inserting of
            // matching Observation Ids into a heap in parallel.
            var matchingObservationTable =
                await TempTableCreator.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(
                context: context,
                cancellationToken: cancellationToken);

            // Generate a "WHERE" clause to limit matched Observations to selected
            // Time Periods, if any.
            var timePeriodsClause = timePeriodQuery != null
                ? GetTimePeriodsClause(timePeriodQuery)
                : default;

            // Generate a "WHERE" clause to limit matched Observations to selected
            // Locations, if any.
            var locationIdsClause = locationIds.Count > 0
                ? await GetLocationsClause(context, locationIds, cancellationToken)
                : default;

            // Generate a "WHERE" clause to limit matched Observations to selected
            // Filter Items, if any.
            var filterItemIdsClause = filterItemIds.Count > 0
                ? await GetSelectedFilterItemIdsClause(context, subjectId, filterItemIds, cancellationToken)
                : default;

            // Insert matching Observation Ids into the heap.
            // Use TABLOCK to allow fast and safe parallel inserts into the table
            // and with less logging.
            // Use MAXDOP to prevent too many workers executing in parallel at once,
            // which can negatively impact performance.
            // Use RECOMPILE to tell the execution plan engine to alter its plan based
            // on fresh statistics.
            var matchingObservationSql = $@"
                    INSERT INTO {matchingObservationTable.Name} WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId " +
                     (timePeriodsClause != null ? $"AND ({timePeriodsClause}) " : "") +
                     (locationIdsClause != null ? $"AND ({locationIdsClause}) " : "") +
                     (filterItemIdsClause != null ? $"AND ({filterItemIdsClause}) " : "") +
                     "OPTION(RECOMPILE, MAXDOP 4);";

            // Add a unique clustered index *after* the heap insert for better performance,
            // as adding before the insert forces a more complex execution plan that orders
            // the inserts and disables parallelism.
            var indexSql = $@"
                CREATE UNIQUE CLUSTERED INDEX [IX_MatchedObservation_Id_{Guid.NewGuid()}]
                ON #MatchedObservation(Id)
                WITH (MAXDOP = 4);";

            var parameters = ListOf(new SqlParameter("subjectId", subjectId));

            return ($"{matchingObservationSql}\n\n{indexSql}", parameters, matchingObservationTable);
        }
        
        private async Task<string> GetSelectedFilterItemIdsClause(
            StatisticsDbContext context,
            Guid subjectId, 
            IList<Guid> filterItemIds,
            CancellationToken cancellationToken)
        {
            var selectedFilterItemIdsByFilter =
                await GetSelectedFilterItemIdsByFilter(context, filterItemIds, subjectId, cancellationToken);

            // This line adds a potential optimisation to the final generated SQL by placing the EXISTS clauses
            // with the least number of selected Filter Item Ids first, thus attempting to narrow down the list
            // of candidate rows for the next EXISTS clause in line to match against, and so on and so on.
            //
            // This could also be done by determining the ratio of selected Filter Item Ids for a given Filter
            // versus the number of potential options for that Filter.  A Filter with 5,000 possible Filter Items
            // but only one of them selected would no doubt be very selective too before passing to the next EXISTS
            // clause, but if ratios are being used to determine the order, then if 2,500 options are selected for
            // this Filter that would give a ratio of 50% selected, and when compared to another Filter with 10
            // possible options and 6 selected, the 5,000-options Filter clause would be placed first in the EXISTS
            // chain, which would be less efficient.
            //
            // Thus for now we simply order by the least number of selected Filter Items first.
            var selectedFilterItemsInLeastOptionsOrder = selectedFilterItemIdsByFilter
                .Where(filterItemIdsForFilter => !filterItemIdsForFilter.Value.IsNullOrEmpty())
                .OrderBy(filterItemIdsForFilter => filterItemIdsForFilter.Value.Count);
            
            var filterItemIdTempTablesPerFilter = selectedFilterItemsInLeastOptionsOrder
                .ToDictionary(
                    filterItemIdsForFilter => filterItemIdsForFilter.Key,
                    filterItemIdsForFilter =>
                    {
                        var ids = filterItemIdsForFilter
                            .Value
                            .OrderBy(id => id)
                            .Select(id => new IdTempTable(id))
                            .ToList();

                        return TempTableCreator
                            .CreateAnonymousTemporaryTableAndPopulate(context, ids, cancellationToken).Result;
                    });

            var clauses = filterItemIdTempTablesPerFilter
                .Select(filterItemIdTempTableForFilter =>
                {
                    var filterItemIdsTempTableName =
                        filterItemIdTempTableForFilter.Value.Name;
                    
                    return $"EXISTS (" +
                           $"    SELECT 1 FROM ObservationFilterItem ofi WHERE ofi.ObservationId = o.id " +
                           $"    AND ofi.FilterItemId IN (SELECT Id FROM {filterItemIdsTempTableName})" +
                           $")";
                });

            return clauses.JoinToString(" AND ");
        }

        private static async Task<IDictionary<Guid, List<Guid>>> GetSelectedFilterItemIdsByFilter(
            StatisticsDbContext context,
            IList<Guid> filterItemIds,
            Guid subjectId,
            CancellationToken cancellationToken)
        {
            var filtersForSubject = await context
                .Filter
                .Include(filter => filter.FilterGroups)
                .ThenInclude(filterGroup => filterGroup.FilterItems)
                .Where(filterItem => filterItem.SubjectId == subjectId)
                .ToListAsync(cancellationToken);

            return filtersForSubject
                .ToDictionary(
                    filter => filter.Id,
                    filter =>
                    {
                        var allFilterItemIdsForFilter = filter
                            .FilterGroups
                            .SelectMany(f => f.FilterItems)
                            .Select(f => f.Id);

                        return allFilterItemIdsForFilter
                            .Intersect(filterItemIds)
                            .ToList();
                    });
        }

        private async Task<string> GetLocationsClause(
            StatisticsDbContext context, 
            IList<Guid> locationIds,
            CancellationToken cancellationToken)
        {
            var locationsTempTable = await TempTableCreator.CreateAnonymousTemporaryTableAndPopulate(
                context, locationIds.Select(id => new IdTempTable(id)), cancellationToken);

            return $"o.LocationId IN (SELECT Id FROM {locationsTempTable.Name})";
        }

        private static string GetTimePeriodsClause(TimePeriodQuery timePeriodQuery)
        {
            var timePeriods = TimePeriodUtil.Range(timePeriodQuery).ToList();
            var timePeriodClauses = timePeriods.Select(timePeriod =>
                $"(o.TimeIdentifier = '{timePeriod.TimeIdentifier.GetEnumValue()}' AND o.Year = {timePeriod.Year})");
            return timePeriodClauses.JoinToString(" OR ");
        }
    }
}
