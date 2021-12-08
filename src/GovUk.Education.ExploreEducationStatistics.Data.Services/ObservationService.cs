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

            await batchesOfIds.ForEachAsync(async batchOfIds =>
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
            });

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
            public async Task<Guid[]> GetMatchingObservationIdsQuery(
                StatisticsDbContext context,
                ObservationQueryContext query, 
                CancellationToken cancellationToken)
            {
                var locationsQuery = query.Locations;

                var localAuthorityOldCodes =
                    locationsQuery?.LocalAuthority?.Where(s => s.Length == 3).ToList() ?? new List<string>();
                var localAuthorityCodes = locationsQuery?.LocalAuthority?.Except(localAuthorityOldCodes).ToList();

                var subjectIdParam = new SqlParameter("subjectId", query.SubjectId);
                var geographicLevelParam = new SqlParameter("geographicLevel",
                    locationsQuery?.GeographicLevel?.GetEnumValue() ?? (object)DBNull.Value);
                var timePeriodListParam = CreateTimePeriodListType("timePeriodList", GetTimePeriodRange(query));
                var countriesListParam = CreateIdListType("countriesList", locationsQuery?.Country);
                var englishDevolvedAreaListParam = CreateIdListType("englishDevolvedAreaList", locationsQuery?.EnglishDevolvedArea);
                var institutionListParam =
                    CreateIdListType("institutionList", locationsQuery?.Institution);
                var localAuthorityListParam = CreateIdListType("localAuthorityList", localAuthorityCodes);
                var localAuthorityOldCodeListParam = CreateIdListType("localAuthorityOldCodeList", localAuthorityOldCodes);
                var localAuthorityDistrictListParam =
                    CreateIdListType("localAuthorityDistrictList", locationsQuery?.LocalAuthorityDistrict);
                var localEnterprisePartnershipListParam =
                    CreateIdListType("localEnterprisePartnershipList", locationsQuery?.LocalEnterprisePartnership);
                var mayoralCombinedAuthorityListParam =
                    CreateIdListType("mayoralCombinedAuthorityList", locationsQuery?.MayoralCombinedAuthority);
                var multiAcademyTrustListParam =
                    CreateIdListType("multiAcademyTrustList", locationsQuery?.MultiAcademyTrust);
                var opportunityAreaListParam =
                    CreateIdListType("opportunityAreaList", locationsQuery?.OpportunityArea);
                var parliamentaryConstituencyListParam =
                    CreateIdListType("parliamentaryConstituencyList", locationsQuery?.ParliamentaryConstituency);
                var providersListParam =
                    CreateIdListType("providersList", locationsQuery?.Provider);
                var planningAreaListParam =
                    CreateIdListType("planningAreaList", locationsQuery?.PlanningArea);
                var regionsListParam = CreateIdListType("regionsList", locationsQuery?.Region);
                var rscRegionListParam = CreateIdListType("rscRegionsList", locationsQuery?.RscRegion);
                var schoolsListParam = CreateIdListType("schoolsList", locationsQuery?.School);
                var sponsorListParam = CreateIdListType("sponsorList", locationsQuery?.Sponsor);
                var wardListParam =
                    CreateIdListType("wardList", locationsQuery?.Ward);
                var filterItemListParam = CreateIdListType("filterItemList", query.Filters);

                // EES-745 It's ok to use Observation as the return type here, as long as only the Id field is selected

                var inner = context
                    .Set<Observation>()
                    .FromSqlRaw("EXEC dbo.FilteredObservations " +
                                "@subjectId," +
                                "@geographicLevel," +
                                "@timePeriodList," +
                                "@countriesList," +
                                "@englishDevolvedAreaList," +
                                "@institutionList," +
                                "@localAuthorityList," +
                                "@localAuthorityOldCodeList," +
                                "@localAuthorityDistrictList," +
                                "@localEnterprisePartnershipList," +
                                "@mayoralCombinedAuthorityList," +
                                "@multiAcademyTrustList," +
                                "@opportunityAreaList," +
                                "@parliamentaryConstituencyList," +
                                "@providersList," +
                                "@regionsList," +
                                "@rscRegionsList," +
                                "@schoolsList," +
                                "@sponsorList," +
                                "@wardList," +
                                "@planningAreaList," +
                                "@filterItemList",
                        subjectIdParam,
                        geographicLevelParam,
                        timePeriodListParam,
                        countriesListParam,
                        englishDevolvedAreaListParam,
                        institutionListParam,
                        localAuthorityListParam,
                        localAuthorityOldCodeListParam,
                        localAuthorityDistrictListParam,
                        localEnterprisePartnershipListParam,
                        mayoralCombinedAuthorityListParam,
                        multiAcademyTrustListParam,
                        opportunityAreaListParam,
                        parliamentaryConstituencyListParam,
                        providersListParam,
                        regionsListParam,
                        rscRegionListParam,
                        schoolsListParam,
                        sponsorListParam,
                        wardListParam,
                        planningAreaListParam,
                        filterItemListParam);

                return await inner
                    .Select(obs => obs.Id)
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
