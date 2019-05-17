using System;
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
    public class ObservationService : AbstractDataService<Observation, long>, IObservationService
    {
        private readonly ILocationService _locationService;

        public ObservationService(
            ApplicationDbContext context,
            ILocationService locationService,
            ILogger<ObservationService> logger) : base(context, logger)
        {
            _locationService = locationService;
        }

        public IEnumerable<Observation> FindObservations(ObservationQueryContext query)
        {
            var yearsRange = TimePeriodUtil.YearsRange(query.Years, query.StartYear, query.EndYear);

            var subjectIdParam = new SqlParameter("subjectId", query.SubjectId);
            var geographicLevelParam = new SqlParameter("geographicLevel", query.GeographicLevel.GetEnumValue());
            var yearsListParam = CreateIdListType("yearList", yearsRange);
            var countriesListParam = CreateIdListType("countriesList", query.Countries);
            var regionsListParam = CreateIdListType("regionsList", query.Regions);
            var localAuthorityListParam = CreateIdListType("localAuthorityList", query.LocalAuthorities);
            var localAuthorityDistrictListParam =
                CreateIdListType("localAuthorityDistrictList", query.LocalAuthorityDistricts);
            var localEnterprisePartnershipListParam =
                CreateIdListType("localEnterprisePartnershipList", query.LocalEnterprisePartnerships);
            var institutionListParam =
                CreateIdListType("institutionList", query.Institutions);
            var matListParam =
                CreateIdListType("matList", query.Mats);
            var mayoralCombinedAuthorityListParam =
                CreateIdListType("mayoralCombinedAuthorityList", query.MayoralCombinedAuthorities);
            var opportunityAreaListParam =
                CreateIdListType("opportunityAreaList", query.OpportunityAreas);
            var parliamentaryConstituencyListParam =
                CreateIdListType("parliamentaryConstituencyList", query.ParliamentaryConstituencies);
            var providerListParam =
                CreateIdListType("providerList", query.Providers);
            var wardListParam =
                CreateIdListType("wardList", query.Wards);
            var filtersListParam = CreateIdListType("filtersList", query.Filters);

            var inner = _context.Query<IdWrapper>().AsNoTracking()
                .FromSql("EXEC dbo.FilteredObservations " +
                         "@subjectId," +
                         "@geographicLevel," +
                         "@yearList," +
                         "@countriesList," +
                         "@regionsList," +
                         "@localAuthorityList," +
                         "@localAuthorityDistrictList," +
                         "@localEnterprisePartnershipList," +
                         "@institutionList," +
                         "@matList," +
                         "@mayoralCombinedAuthorityList," +
                         "@opportunityAreaList," +
                         "@parliamentaryConstituencyList," +
                         "@providerList," +
                         "@wardList," +
                         "@filtersList",
                    subjectIdParam,
                    geographicLevelParam,
                    yearsListParam,
                    countriesListParam,
                    regionsListParam,
                    localAuthorityListParam,
                    localAuthorityDistrictListParam,
                    localEnterprisePartnershipListParam,
                    institutionListParam,
                    matListParam,
                    mayoralCombinedAuthorityListParam,
                    opportunityAreaListParam,
                    parliamentaryConstituencyListParam,
                    providerListParam,
                    wardListParam,
                    filtersListParam);

            var ids = inner.Select(obs => obs.Id).ToList();

            var result = DbSet()
                .Where(observation => ids.Contains(observation.Id))
                .Include(observation => observation.Subject)
                .Include(observation => observation.Location)
                .Include(observation => observation.FilterItems)
                .ThenInclude(item => item.FilterItem.FilterGroup);

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

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnitsMeta(
            SubjectMetaQueryContext query)
        {
            var locations = GetLocations(query);

            return new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>
            {
                {
                    GeographicLevel.National,
                    GroupByObservationalUnit(locations, location => location.Country)
                },
                {
                    GeographicLevel.Regional,
                    GroupByObservationalUnit(locations, location => location.Region)
                },
                {
                    GeographicLevel.Local_Authority,
                    GroupByObservationalUnit(locations, location => location.LocalAuthority)
                },
                {
                    GeographicLevel.Local_Authority_District,
                    GroupByObservationalUnit(locations, location => location.LocalAuthorityDistrict)
                },
                {
                    GeographicLevel.Local_Enterprise_Partnerships,
                    GroupByObservationalUnit(locations, location => location.LocalEnterprisePartnership)
                },
                {
                    GeographicLevel.Institution,
                    GroupByObservationalUnit(locations, location => location.Institution)
                },
                {
                    GeographicLevel.MAT_Or_Sponsor,
                    GroupByObservationalUnit(locations, location => location.Mat)
                },
                {
                    GeographicLevel.Mayoral_Combined_Authorities,
                    GroupByObservationalUnit(locations, location => location.MayoralCombinedAuthority)
                },
                {
                    GeographicLevel.Opportunity_Areas,
                    GroupByObservationalUnit(locations, location => location.OpportunityArea)
                },
                {
                    GeographicLevel.Parliamentary_Constituency,
                    GroupByObservationalUnit(locations, location => location.ParliamentaryConstituency)
                },
                {
                    GeographicLevel.Provider,
                    GroupByObservationalUnit(locations, location => location.Provider)
                },
                {
                    GeographicLevel.Ward,
                    GroupByObservationalUnit(locations, location => location.Ward)
                }
            };
        }

        private IEnumerable<Location> GetLocations(SubjectMetaQueryContext query)
        {
            var locationIds = DbSet()
                .AsNoTracking()
                .Where(query.ObservationPredicate())
                .GroupBy(observation => observation.LocationId)
                .Select(group => group.Key);

            return locationIds.Any() ? _locationService.Find(locationIds.ToArray()) : new List<Location>();
        }

        private static IEnumerable<T> GroupByObservationalUnit<T>(IEnumerable<Location> locations,
            Func<Location, T> keySelector) where T : IObservationalUnit
        {
            return locations.GroupBy(keySelector)
                .Where(grouping => grouping.Key.Code != null)
                .Select(group => group.Key);
        }
    }
}