using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
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
            var yearsRange = TimePeriodUtil.YearsRange(query.Years, query.StartYear, query.EndYear);

            var subjectIdParam = new SqlParameter("subjectId", query.SubjectId);
            var geographicLevelParam = new SqlParameter("geographicLevel", query.GeographicLevel.GetEnumValue());
            var yearsListParam = CreateIdListType("yearList", yearsRange);
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
                         "@yearList," +
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
                    yearsListParam,
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

            var result = DbSet()
                .AsNoTracking()
                .Where(observation => ids.Contains(observation.Id))
                .Include(observation => observation.Subject)
                .ThenInclude(subject => subject.Release)
                .Include(observation => observation.Location)
                .Include(observation => observation.FilterItems)
                .ThenInclude(item => item.FilterItem.FilterGroup).ToList();

            result.Select(observation => observation.Location)
                .Distinct()
                .ToList()
                .ForEach(location => location.ReplaceEmptyOwnedTypeValuesWithNull());

            return result;
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<int> idList)
        {
            return CreateListType(parameterName, idList.AsIdListTable(), "dbo.IdListIntegerType");
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<long> idList)
        {
            return CreateListType(parameterName, idList.AsIdListTable(), "dbo.IdListIntegerType");
        }

        private static SqlParameter CreateIdListType(string parameterName, IEnumerable<string> idList)
        {
            return CreateListType(parameterName, idList.AsIdListTable(), "dbo.IdListVarcharType");
        }

        private static SqlParameter CreateListType(string parameterName, object value, string typeName)
        {
            return new SqlParameter(parameterName, value)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName
            };
        }

        public IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query)
        {
            var timePeriods = (from o in DbSet().AsNoTracking().Where(query.ObservationPredicate())
                select new {o.TimeIdentifier, o.Year}).Distinct();

            return from timePeriod in timePeriods.AsEnumerable()
                select (timePeriod.TimeIdentifier, timePeriod.Year);
        }
    }
}