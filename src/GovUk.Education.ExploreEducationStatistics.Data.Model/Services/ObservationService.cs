using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.ViewModels;
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

        public IEnumerable<Observation> FindObservations(long subjectId,
            GeographicLevel geographicLevel,
            IEnumerable<int> years,
            IEnumerable<string> countries,
            IEnumerable<string> regions,
            IEnumerable<string> localAuthorities,
            IEnumerable<string> localAuthorityDistricts,
            IEnumerable<long> filters)
        {
            var subjectIdParam = new SqlParameter("subjectId", subjectId);
            var geographicLevelParam = new SqlParameter("geographicLevel", geographicLevel.GetEnumLabel());
            var yearsListParam = CreateIdListType("yearList", years);
            var countriesListParam = CreateIdListType("countriesList", countries);
            var regionsListParam = CreateIdListType("regionsList", regions);
            var localAuthorityListParam = CreateIdListType("localAuthorityList", localAuthorities);
            var localAuthorityDistrictListParam =
                CreateIdListType("localAuthorityDistrictList", localAuthorityDistricts);
            var filtersListParam = CreateIdListType("filtersList", filters);
            
            var inner = _context.Query<IdWrapper>().AsNoTracking()
                .FromSql("EXEC dbo.FilteredObservations " +
                         "@subjectId," +
                         "@geographicLevel," +
                         "@yearList," +
                         "@countriesList," +
                         "@regionsList," +
                         "@localAuthorityList," +
                         "@localAuthorityDistrictList," +
                         "@filtersList",
                    subjectIdParam,
                    geographicLevelParam,
                    yearsListParam,
                    countriesListParam,
                    regionsListParam,
                    localAuthorityListParam,
                    localAuthorityDistrictListParam,
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

        public IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId)
        {
            var timePeriods = (from o in DbSet().Where(data => data.SubjectId == subjectId)
                select new {o.TimeIdentifier, o.Year}).Distinct();

            return from timePeriod in timePeriods.AsEnumerable()
                select (timePeriod.TimeIdentifier, timePeriod.Year);
        }

        public LocationMeta GetLocationMeta(long subjectId)
        {
            var locations = GetLocations(subjectId);
            return new LocationMeta
            {
                Country = locations.GroupBy(composite => composite.Country)
                    .Where(grouping => grouping.Key != null)
                    .Select(group => group.Key),
                LocalAuthority = locations.GroupBy(composite => composite.LocalAuthority)
                    .Where(grouping => grouping.Key.Code != null)
                    .Select(group => group.Key),
                LocalAuthorityDistrict = locations.GroupBy(composite => composite.LocalAuthorityDistrict)
                    .Where(grouping => grouping.Key.Code != null)
                    .Select(group => group.Key),
                Region = locations.GroupBy(composite => composite.Region)
                    .Where(grouping => grouping.Key.Code != null)
                    .Select(group => group.Key)
            };
        }

        private IEnumerable<Location> GetLocations(long subjectId)
        {
//            TODO Ideally want one db query as follows but this is translated into invalid SQL
//            TODO See https://github.com/aspnet/EntityFrameworkCore/issues/12304
//            return (from l in _context.Set<Location>()
//                join
//                    d in _context.Observation.Where(data => data.SubjectId == subjectId)
//                        .GroupBy(data => data.LocationId) on l.Id equals d.Key
//                select l).ToList();

            var locationIds = DbSet().Where(data => data.SubjectId == subjectId)
                .GroupBy(observation => observation.LocationId)
                .Select(group => group.Key);

            return locationIds.Any() ? _locationService.Find(locationIds.ToArray()) : new List<Location>();
        }
    }
}