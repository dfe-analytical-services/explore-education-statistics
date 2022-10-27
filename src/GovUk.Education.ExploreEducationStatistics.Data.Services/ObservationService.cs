#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture;
using Thinktecture.EntityFrameworkCore.TempTables;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
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

        public async Task<IQueryable<MatchedObservation>> GetMatchedObservations(
            ObservationQueryContext query,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            var (sql, sqlParameters, tempTables) = await QueryGenerator
                .GetMatchingObservationsQuery(
                    _context,
                    query.SubjectId,
                    query.Filters?.ToList(),
                    query.LocationIds,
                    query.TimePeriod,
                    cancellationToken);

            try
            {
                await SqlExecutor.ExecuteSqlRaw(_context, sql, sqlParameters, cancellationToken);

                var matchedObservations = _context
                    .MatchedObservations
                    .AsNoTracking();

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Finished fetching {ObservationCount} Observations in a total of " +
                                     "{Milliseconds} ms", matchedObservations.Count(), sw.Elapsed.TotalMilliseconds);
                }

                return matchedObservations;
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
        
        public interface IMatchingObservationsQueryGenerator
        {
            Task<(string, IList<SqlParameter>, IList<IAsyncDisposable>)> GetMatchingObservationsQuery(
                StatisticsDbContext context,
                Guid subjectId,
                IList<Guid>? filterItemIds,
                IList<Guid>? locationIds,
                TimePeriodQuery? timePeriodQuery,
                CancellationToken cancellationToken);
        }

        public class MatchingObservationsQueryGenerator : IMatchingObservationsQueryGenerator
        {
            public ITemporaryTableCreator TempTableCreator = new TemporaryTableCreator();
            private readonly Regex _safeTempTableNames = new("^#[a-zA-Z0-9]+[_0-9]*$", RegexOptions.Compiled);

            public async Task<(string, IList<SqlParameter>, IList<IAsyncDisposable>)> GetMatchingObservationsQuery(
                StatisticsDbContext context,
                Guid subjectId,
                IList<Guid>? filterItemIds,
                IList<Guid>? locationIds,
                TimePeriodQuery? timePeriodQuery,
                CancellationToken cancellationToken)
            {
                await TempTableCreator.CreateTemporaryTable<MatchedObservation>(context, cancellationToken);

                var (locationIdsClause, locationIdsTempTable) = !locationIds.IsNullOrEmpty()
                    ? await GetLocationsClause(context, locationIds!, cancellationToken)
                    : default;

                var (filterItemIdsClause, filterItemIdTempTables) = !filterItemIds.IsNullOrEmpty()
                    ? await GetSelectedFilterItemIdsClause(context, subjectId, filterItemIds!, cancellationToken)
                    : default;

                var sql = @$"
                    INSERT INTO #{nameof(MatchedObservation)} 
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId " +
                    (timePeriodQuery != null ? $"AND ({GetTimePeriodsClause(timePeriodQuery)}) " : "") +
                    (locationIdsClause != null ? $"AND ({locationIdsClause}) " : "") +
                    (filterItemIdsClause != null ? $"AND ({filterItemIdsClause}) " : "") +
                    "ORDER BY o.Id;";

                var parameters = ListOf(new SqlParameter("subjectId", subjectId));
                
                var tableReferences = new List<IAsyncDisposable>();
                
                if (locationIdsTempTable != null) {
                    tableReferences.Add(locationIdsTempTable);
                }
                
                if (!filterItemIdTempTables.IsNullOrEmpty())
                {
                    tableReferences.AddRange(filterItemIdTempTables);
                }
                
                return (sql, parameters, tableReferences);
            }
            
            private async Task<(string, List<ITempTableQuery<IdTempTable>>)> GetSelectedFilterItemIdsClause(
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
                            
                            return TempTableCreator.CreateTemporaryTableAndPopulate(context, ids, cancellationToken).Result;
                        });

                var clauses = filterItemIdTempTablesPerFilter
                    .Select(filterItemIdTempTableForFilter =>
                    {
                        var filterItemIdsTempTableName = 
                            SanitizeTempTableName(filterItemIdTempTableForFilter.Value.Name);
                        
                        return $"EXISTS (" +
                               $"    SELECT 1 FROM ObservationFilterItem ofi WHERE ofi.ObservationId = o.id " +
                               $"    AND ofi.FilterItemId IN (SELECT Id FROM {filterItemIdsTempTableName})" +
                               $")";
                    });

                return (clauses.JoinToString(" AND "), filterItemIdTempTablesPerFilter.Values.ToList());
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

            private async Task<(string, ITempTableQuery<IdTempTable>)> GetLocationsClause(
                StatisticsDbContext context, 
                IList<Guid> locationIds,
                CancellationToken cancellationToken)
            {
                var locationsTempTable = await TempTableCreator.CreateTemporaryTableAndPopulate(
                    context, locationIds.Select(id => new IdTempTable(id)), cancellationToken);
                
                return ($"o.LocationId IN (SELECT Id FROM {SanitizeTempTableName(locationsTempTable.Name)})", locationsTempTable);
            }

            private string SanitizeTempTableName(string tempTableName)
            {
                if (!_safeTempTableNames.IsMatch(tempTableName))
                {
                    throw new ArgumentException($"{tempTableName} is not a valid temporary table name");
                }
                return tempTableName;
            }

            private static string GetTimePeriodsClause(TimePeriodQuery timePeriodQuery)
            {
                var timePeriods = TimePeriodUtil.Range(timePeriodQuery).ToList();
                var timePeriodClauses = timePeriods.Select(timePeriod =>
                    $"(o.TimeIdentifier = '{timePeriod.TimeIdentifier.GetEnumValue()}' AND o.Year = {timePeriod.Year})");
                return timePeriodClauses.JoinToString(" OR ");
            }

            public interface ITemporaryTableCreator
            {
                Task CreateTemporaryTable<TEntity>(
                    StatisticsDbContext context,
                    CancellationToken cancellationToken) where TEntity : class;
            
                Task<ITempTableQuery<TEntity>> CreateTemporaryTableAndPopulate<TEntity>(
                    StatisticsDbContext context,
                    IEnumerable<TEntity> values,
                    CancellationToken cancellationToken) where TEntity : class;
            }
            
            public class TemporaryTableCreator : ITemporaryTableCreator
            {
                public async Task CreateTemporaryTable<TEntity>(
                    StatisticsDbContext context, 
                    CancellationToken cancellationToken) where TEntity : class
                {
                    var options = new TempTableCreationOptions
                    {
                       TableNameProvider = new DefaultTempTableNameProvider(),
                    };

                    await context.CreateTempTableAsync<TEntity>(
                        options,
                        cancellationToken);
                }

                public async Task<ITempTableQuery<TEntity>> CreateTemporaryTableAndPopulate<TEntity>(
                    StatisticsDbContext context, 
                    IEnumerable<TEntity> values,
                    CancellationToken cancellationToken) where TEntity : class
                {
                    return await context.BulkInsertIntoTempTableAsync(values, cancellationToken: cancellationToken);
                }
            }
        }

        public interface IRawSqlExecutor
        {
            Task ExecuteSqlRaw(
                StatisticsDbContext context,
                string sql,
                IList<SqlParameter> parameters,
                CancellationToken cancellationToken);
        }

        public class RawSqlExecutor : IRawSqlExecutor
        {
            public async Task ExecuteSqlRaw(
                StatisticsDbContext context, 
                string sql, 
                IList<SqlParameter> parameters, 
                CancellationToken cancellationToken)
            {
                await context
                    .Database
                    .ExecuteSqlRawAsync(sql, parameters, cancellationToken);
            }
        }
    }
}
