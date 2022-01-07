using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ObservationService : AbstractRepository<Observation, long>, IObservationService
    {
        private const int ObservationFetchBatchSize = 100;

        private readonly ILogger<ObservationService> _logger;
        public IMatchingObservationsGetter MatchingObservationGetter { get; set; } = new MatchingObservationsGetter();

        public ObservationService(
            StatisticsDbContext context,
            ILogger<ObservationService> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IList<Observation>> FindObservations(
            ObservationQueryContext query,
            CancellationToken cancellationToken = default)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var phasesStopwatch = Stopwatch.StartNew();

            var ids = await MatchingObservationGetter
                .GetMatchingObservationIdsQuery(_context, query, cancellationToken);

            _logger.LogDebug($"Executed FilteredObservations stored procedure in " +
                             $"{phasesStopwatch.Elapsed.TotalMilliseconds} ms - fetched {ids.Length} " +
                             $"Observation ids");
            phasesStopwatch.Restart();

            var batchesOfIds = ids.Batch(ObservationFetchBatchSize).ToList();

            var observations = new List<Observation>();

            await batchesOfIds
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async batchOfIds =>
                {
                    var observationBatch = await _context
                        .Observation
                        .AsNoTracking()
                        .Include(o => o.FilterItems)
                        .Include(o => o.Location)
                        .Where(o => batchOfIds.Contains(o.Id))
                        .ToListAsync(cancellationToken);

                    observations.AddRange(observationBatch);

                    _logger.LogDebug($"Fetched batch of {observationBatch.Count} Observations from their ids in " +
                                     $"{phasesStopwatch.Elapsed.TotalMilliseconds} ms");
                    phasesStopwatch.Restart();
                }, cancellationToken: cancellationToken);

            _logger.LogDebug($"Finished fetching {ids.Length} Observations in a total of " +
                             $"{totalStopwatch.Elapsed.TotalMilliseconds} ms");
            return observations;
        }

        public IQueryable<Observation> FindObservations(SubjectMetaQueryContext query)
        {
            return DbSet()
                .AsNoTracking()
                .Include(observation => observation.FilterItems)
                .Where(ObservationPredicateBuilder.Build(query));
        }

        public interface IMatchingObservationsGetter
        {
            Task<Guid[]> GetMatchingObservationIdsQuery(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken);
        }

        private class MatchingObservationsGetter : IMatchingObservationsGetter
        {
            public Task<Guid[]> GetMatchingObservationIdsQuery(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken)
            {
                if (query.LocationIds.IsNullOrEmpty())
                {
                    // Support old Data Blocks that have Location codes rather than id's in their query
                    // TODO EES-3068 Migrate Location codes to ids in old Datablocks to remove this support for Location codes
                    return GetMatchingObservationIdsByLocationCodes(context, query, cancellationToken);
                }
                return GetMatchingObservationIds(context, query, cancellationToken);
            }

            private static async Task<Guid[]> GetMatchingObservationIds(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken)
            {
                var subjectId = new SqlParameter("subjectId", query.SubjectId);
                var filterItemIds = CreateIdListType("filterItemIds", query.Filters);
                var locationIds = CreateIdListType("locationIds", query.LocationIds);
                var timePeriods = CreateTimePeriodListType("timePeriods", GetTimePeriodRange(query));

                const string sql = "EXEC dbo.SelectObservations @subjectId,@filterItemIds,@locationIds,@timePeriods";

                // EES-745 It's ok to use Observation as the return type here, as long as only the Id field is selected
                // ReSharper disable FormatStringProblem
                return await context
                    .Set<Observation>()
                    .FromSqlRaw(sql,
                        subjectId,
                        filterItemIds,
                        locationIds,
                        timePeriods)
                    .Select(observation => observation.Id)
                    .ToArrayAsync(cancellationToken);
            }

            private static async Task<Guid[]> GetMatchingObservationIdsByLocationCodes(
                StatisticsDbContext context,
                ObservationQueryContext query,
                CancellationToken cancellationToken)
            {
                var locationsQuery = query.Locations;

                var subjectIdParam = new SqlParameter("subjectId", query.SubjectId);
                var filterItemIds = CreateIdListType("filterItemIds", query.Filters);
                var timePeriods = CreateTimePeriodListType("timePeriods", GetTimePeriodRange(query));
                var geographicLevel = new SqlParameter("geographicLevel",
                    locationsQuery?.GeographicLevel?.GetEnumValue() ?? (object) DBNull.Value);

                var localAuthorityOldCodesList =
                    locationsQuery?.LocalAuthority?.Where(s => s.Length == 3).ToList() ?? new List<string>();
                var localAuthorityCodesList = locationsQuery?.LocalAuthority?.Except(localAuthorityOldCodesList).ToList();

                var countries =
                    CreateIdListType("countries", locationsQuery?.Country);
                var englishDevolvedAreas =
                    CreateIdListType("englishDevolvedAreas", locationsQuery?.EnglishDevolvedArea);
                var institutions =
                    CreateIdListType("institutions", locationsQuery?.Institution);
                var localAuthorities =
                    CreateIdListType("localAuthorities", localAuthorityCodesList);
                var localAuthorityOldCodes =
                    CreateIdListType("localAuthorityOldCodes", localAuthorityOldCodesList);
                var localAuthorityDistricts =
                    CreateIdListType("localAuthorityDistricts", locationsQuery?.LocalAuthorityDistrict);
                var localEnterprisePartnerships =
                    CreateIdListType("localEnterprisePartnerships", locationsQuery?.LocalEnterprisePartnership);
                var mayoralCombinedAuthorities =
                    CreateIdListType("mayoralCombinedAuthorities", locationsQuery?.MayoralCombinedAuthority);
                var multiAcademyTrusts =
                    CreateIdListType("multiAcademyTrusts", locationsQuery?.MultiAcademyTrust);
                var opportunityAreas =
                    CreateIdListType("opportunityAreas", locationsQuery?.OpportunityArea);
                var parliamentaryConstituencies =
                    CreateIdListType("parliamentaryConstituencies", locationsQuery?.ParliamentaryConstituency);
                var planningAreas =
                    CreateIdListType("planningAreas", locationsQuery?.PlanningArea);
                var providers =
                    CreateIdListType("providers", locationsQuery?.Provider);
                var regions =
                    CreateIdListType("regions", locationsQuery?.Region);
                var rscRegions =
                    CreateIdListType("rscRegions", locationsQuery?.RscRegion);
                var schools =
                    CreateIdListType("schools", locationsQuery?.School);
                var sponsors =
                    CreateIdListType("sponsors", locationsQuery?.Sponsor);
                var wards =
                    CreateIdListType("wards", locationsQuery?.Ward);

                // EES-745 It's ok to use Observation as the return type here, as long as only the Id field is selected
                // ReSharper disable FormatStringProblem
                const string sql = @"EXEC dbo.SelectObservationsByLocationCodes " +
                                   "@subjectId," +
                                   "@filterItemIds," +
                                   "@timePeriods," +
                                   "@geographicLevel," +
                                   "@countries," +
                                   "@englishDevolvedAreas," +
                                   "@institutions," +
                                   "@localAuthorities," +
                                   "@localAuthorityOldCodes," +
                                   "@localAuthorityDistricts," +
                                   "@localEnterprisePartnerships," +
                                   "@mayoralCombinedAuthorities," +
                                   "@multiAcademyTrusts," +
                                   "@opportunityAreas," +
                                   "@parliamentaryConstituencies," +
                                   "@planningAreas," +
                                   "@providers," +
                                   "@regions," +
                                   "@rscRegions," +
                                   "@schools," +
                                   "@sponsors," +
                                   "@wards";

                var inner = context
                    .Set<Observation>()
                    .FromSqlRaw(sql,
                        subjectIdParam,
                        filterItemIds,
                        timePeriods,
                        geographicLevel,
                        countries,
                        englishDevolvedAreas,
                        institutions,
                        localAuthorities,
                        localAuthorityOldCodes,
                        localAuthorityDistricts,
                        localEnterprisePartnerships,
                        mayoralCombinedAuthorities,
                        multiAcademyTrusts,
                        opportunityAreas,
                        parliamentaryConstituencies,
                        planningAreas,
                        providers,
                        regions,
                        rscRegions,
                        schools,
                        sponsors,
                        wards);

                return await inner
                    .Select(observation => observation.Id)
                    .ToArrayAsync(cancellationToken);
            }
        }

        private static SqlParameter CreateTimePeriodListType(string parameterName,
            IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> values)
        {
            return CreateListType(parameterName, values.AsTimePeriodListTable(), "dbo.TimePeriodListType");
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(ObservationQueryContext query)
        {
            return TimePeriodUtil.Range(query.TimePeriod);
        }
    }
}
