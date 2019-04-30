using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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

        public IEnumerable<Observation> FindObservations(Expression<Func<Observation, bool>> findExpression, IEnumerable<long> filters)
        {            
            var queryable = DbSet().Where(findExpression)
                .Include(observation => observation.Subject)
                .Include(observation => observation.Location)
                .Include(observation => observation.FilterItems)
                .ThenInclude(item => item.FilterItem.FilterGroup);

            return
                from ofi in _context.ObservationFilterItem
                join o in queryable on ofi.ObservationId equals o.Id
                where filters.Contains(ofi.FilterItemId)
                select o;
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