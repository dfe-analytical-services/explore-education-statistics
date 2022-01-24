#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture;
using Thinktecture.EntityFrameworkCore.TempTables;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ObservationService : AbstractRepository<Observation, long>, IObservationService
    {
        private readonly ILogger<ObservationService> _logger;
        public IMatchingObservationsGetter MatchingObservationGetter { get; set; } = new MatchingObservationsGetter();

        public ObservationService(
            StatisticsDbContext context,
            ILogger<ObservationService> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IQueryable<MatchedObservation>> GetMatchedObservations(
            ObservationQueryContext query,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            var observationRows = await MatchingObservationGetter
                .GetMatchingObservationIdsQuery(_context, query, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Finished fetching {ObservationCount} Observations in a total of " +
                                 "{Milliseconds} ms", observationRows.Count(), sw.Elapsed.TotalMilliseconds);
            }

            return observationRows;
        }

        public interface IMatchingObservationsGetter
        {
            Task<IQueryable<MatchedObservation>> GetMatchingObservationIdsQuery(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken);
        }

        private class MatchingObservationsGetter : IMatchingObservationsGetter
        {
            public async Task<IQueryable<MatchedObservation>> GetMatchingObservationIdsQuery(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken)
            {
                IEnumerable<Guid> locationIds;

                // Support old Data Blocks that have Location codes rather than id's in their query
                // TODO EES-3068 Migrate Location codes to ids in old Datablocks to remove this support for Location codes
                if (query.LocationIds.IsNullOrEmpty())
                {
                    locationIds = await context
                        .Location
                        .Where(LocationPredicateBuilder.Build(query.LocationIds, query.Locations))
                        .Select(location => location.Id)
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    locationIds = query.LocationIds;
                }

                return await GetMatchingObservationIds(
                    context,
                    query.SubjectId,
                    query.Filters?.ToList(),
                    locationIds?.ToList(),
                    query.TimePeriod,
                    cancellationToken);
            }

            private static async Task<IQueryable<MatchedObservation>> GetMatchingObservationIds(
                StatisticsDbContext context,
                Guid subjectId,
                IList<Guid>? filterItemIds, 
                IList<Guid>? locationIds,
                TimePeriodQuery? timePeriodQuery,
                CancellationToken cancellationToken)
            {
                await context.CreateTempTableAsync<MatchedObservation>(
                    new DefaultTempTableNameProvider(), true, cancellationToken);

                var (locationIdsClause, locationIdsTempTable) = !locationIds.IsNullOrEmpty() 
                    ? await GetLocationsClause(context, locationIds!, cancellationToken) 
                    : default;
                
                var (filterItemIdsClause, filterItemIdTempTables) = !filterItemIds.IsNullOrEmpty() 
                    ? await GetSelectedFilterItemIdsClause(context, subjectId, filterItemIds!, cancellationToken) 
                    : default;

                try
                {
                    var sql =
                        $"INSERT INTO #{nameof(MatchedObservation)} " +
                        $"SELECT o.id FROM Observation o " +
                        "WHERE o.SubjectId = @subjectId " +
                        (timePeriodQuery != null ? $"AND ({GetTimePeriodsClause(timePeriodQuery)}) " : "") +
                        (locationIdsClause != null ? $"AND ({locationIdsClause}) " : "") +
                        (filterItemIdsClause != null ? $"AND ({filterItemIdsClause})" : "") +
                        "ORDER BY o.Id;";

                    await context
                        .Database
                        .ExecuteSqlRawAsync(
                            sql,
                            ListOf(new SqlParameter("subjectId", subjectId)),
                            cancellationToken);

                    return context
                        .MatchedObservations
                        .AsNoTracking();
                }
                finally
                {
                    if (locationIdsTempTable != null)
                    {
                        await locationIdsTempTable.DisposeAsync();
                    }

                    if (!filterItemIdTempTables.IsNullOrEmpty())
                    {
                        await filterItemIdTempTables
                            .ToAsyncEnumerable()
                            .ForEachAwaitAsync(async table => await table.DisposeAsync(), cancellationToken);
                    }
                }
            }

            private static async Task<(string, List<ITempTableQuery<IdTempTable>>)> GetSelectedFilterItemIdsClause(
                StatisticsDbContext context,
                Guid subjectId, 
                IList<Guid> filterItemIds,
                CancellationToken cancellationToken)
            {
                var selectedFilterItemIdsByFilter =
                    await GetSelectedFilterItemIdsByFilter(context, filterItemIds, subjectId, cancellationToken);
                
                var filterItemIdTempTablesPerFilter = selectedFilterItemIdsByFilter
                    .ToDictionary(
                        filterItemIdsForFilter => filterItemIdsForFilter.Key,
                        filterItemIdsForFilter =>
                        {
                            var ids = filterItemIdsForFilter.Value.Select(id => new IdTempTable(id));
                            return context.BulkInsertIntoTempTableAsync(ids, cancellationToken: cancellationToken).Result; // TODO async
                        });

                // TODO token replacement
                var clauses = filterItemIdTempTablesPerFilter
                    .Select(filterItemIdTempTableForFilter =>
                        $"EXISTS (" +
                        $"    SELECT 1 FROM ObservationFilterItem ofi WHERE ofi.ObservationId = o.id " +
                        $"    AND ofi.FilterItemId IN (SELECT Id FROM {filterItemIdTempTableForFilter.Value.Name})" +
                        $")");

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

            private static async Task<(string, ITempTableQuery<IdTempTable>)> GetLocationsClause(
                StatisticsDbContext context, 
                IList<Guid> locationIds,
                CancellationToken cancellationToken)
            {
                var locationsTempTable = await context.BulkInsertIntoTempTableAsync(
                    locationIds.Select(id => new IdTempTable(id)), cancellationToken: cancellationToken);
                
                // TODO token replacement
                return ("o.LocationId IN (SELECT Id FROM " + locationsTempTable.Name + ")", locationsTempTable);
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
}
