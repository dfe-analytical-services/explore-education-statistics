using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class ObservationService : AbstractRepository<Observation, long>, IObservationService
    {
        public ObservationService(
            ApplicationDbContext context,
            ILogger<ObservationService> logger) : base(context, logger)
        {
        }

        public IEnumerable<Observation> FindObservations(ObservationQueryContext query)
        {
            var subjectIdParam = new SqlParameter("subjectId", query.SubjectId);
            var geographicLevelParam = new SqlParameter("geographicLevel",
                query.GeographicLevel?.GetEnumValue() ?? (object) DBNull.Value);
            var timePeriodListParam = CreateTimePeriodListType("timePeriodList", GetTimePeriodRange(query));
            var countriesListParam = CreateIdListType("countriesList", query.Country);
            var institutionListParam =
                CreateIdListType("institutionList", query.Institution);
            var localAuthorityListParam = CreateIdListType("localAuthorityList", query.LocalAuthority);
            var localAuthorityDistrictListParam =
                CreateIdListType("localAuthorityDistrictList", query.LocalAuthorityDistrict);
            var localEnterprisePartnershipListParam =
                CreateIdListType("localEnterprisePartnershipList", query.LocalEnterprisePartnership);
            var mayoralCombinedAuthorityListParam =
                CreateIdListType("mayoralCombinedAuthorityList", query.MayoralCombinedAuthority);
            var multiAcademyTrustListParam =
                CreateIdListType("multiAcademyTrustList", query.MultiAcademyTrust);
            var opportunityAreaListParam =
                CreateIdListType("opportunityAreaList", query.OpportunityArea);
            var parliamentaryConstituencyListParam =
                CreateIdListType("parliamentaryConstituencyList", query.ParliamentaryConstituency);
            var regionsListParam = CreateIdListType("regionsList", query.Region);
            var rscRegionListParam = CreateIdListType("rscRegionsList", query.RscRegion);
            var sponsorListParam = CreateIdListType("sponsorList", query.Sponsor);
            var wardListParam =
                CreateIdListType("wardList", query.Ward);
            var filtersListParam = CreateIdListType("filtersList", query.Filters);

            var inner = _context.Query<IdWrapper>().AsNoTracking()
                .FromSql("EXEC dbo.FilteredObservations " +
                         "@subjectId," +
                         "@geographicLevel," +
                         "@timePeriodList," +
                         "@countriesList," +
                         "@institutionList," +
                         "@localAuthorityList," +
                         "@localAuthorityDistrictList," +
                         "@localEnterprisePartnershipList," +
                         "@mayoralCombinedAuthorityList," +
                         "@multiAcademyTrustList," +
                         "@opportunityAreaList," +
                         "@parliamentaryConstituencyList," +
                         "@regionsList," +
                         "@rscRegionsList," +
                         "@sponsorList," +
                         "@wardList," +
                         "@filtersList",
                    subjectIdParam,
                    geographicLevelParam,
                    timePeriodListParam,
                    countriesListParam,
                    institutionListParam,
                    localAuthorityListParam,
                    localAuthorityDistrictListParam,
                    localEnterprisePartnershipListParam,
                    mayoralCombinedAuthorityListParam,
                    multiAcademyTrustListParam,
                    opportunityAreaListParam,
                    parliamentaryConstituencyListParam,
                    regionsListParam,
                    rscRegionListParam,
                    sponsorListParam,
                    wardListParam,
                    filtersListParam);

            var ids = inner.Select(obs => obs.Id).ToList();
            var batches = ids.Batch(10000);

            var result = new List<Observation>();

            foreach (var batch in batches)
            {
                result.AddRange(DbSet()
                    .AsNoTracking()
                    .Where(observation => batch.Contains(observation.Id))
                    .Include(observation => observation.Subject)
                    .ThenInclude(subject => subject.Release)
                    .Include(observation => observation.Location)
                    .Include(observation => observation.FilterItems)
                    .ThenInclude(item => item.FilterItem.FilterGroup));
            }

            result.Select(observation => observation.Location)
                .Distinct()
                .ToList()
                .ForEach(location => location.ReplaceEmptyOwnedTypeValuesWithNull());

            return result;
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<int> values)
        {
            return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListIntegerType");
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<long> values)
        {
            return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListIntegerType");
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<string> values)
        {
            return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListVarcharType");
        }

        private static SqlParameter CreateTimePeriodListType(string parameterName,
            IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> values)
        {
            return CreateListType(parameterName, values.AsTimePeriodListTable(), "dbo.TimePeriodListType");
        }

        private static SqlParameter CreateListType(string parameterName, object value, string typeName)
        {
            return new SqlParameter(parameterName, value)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName
            };
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            ObservationQueryContext query)
        {
            if (query.TimePeriod.StartCode.IsNumberOfTerms() || query.TimePeriod.EndCode.IsNumberOfTerms())
            {
                return TimePeriodUtil.RangeForNumberOfTerms(query.TimePeriod.StartYear, query.TimePeriod.EndYear);
            }

            return TimePeriodUtil.Range(query.TimePeriod);
        }

        public IEnumerable<(TimeIdentifier TimeIdentifier, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query)
        {
            var timePeriods = DbSet().AsNoTracking().Where(query.ObservationPredicate())
                .Select(o => new {o.TimeIdentifier, o.Year})
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .Distinct();

            return from timePeriod in timePeriods.AsEnumerable()
                select (timePeriod.TimeIdentifier, timePeriod.Year);
        }
    }
}