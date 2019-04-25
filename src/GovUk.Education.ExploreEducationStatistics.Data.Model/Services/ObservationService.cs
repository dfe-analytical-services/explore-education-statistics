using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.ViewModels;
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

        public IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId)
        {
            var timePeriods = (from o in DbSet().Where(data => data.SubjectId == subjectId)
                select new {o.TimeIdentifier, o.Year }).Distinct();
            
            return from timePeriod in timePeriods.AsEnumerable()
                select (timePeriod.TimeIdentifier, timePeriod.Year);
        }

        public LocationMeta GetLocationMeta(long subjectId)
        {
            var locations = GetLocations(subjectId);
            return new LocationMeta
            {
                Country = locations.GroupBy(composite => composite.Country).Select(group => group.Key),
                LocalAuthority = locations.GroupBy(composite => composite.LocalAuthority).Select(group => group.Key),
                LocalAuthorityDistrict = locations.GroupBy(composite => composite.LocalAuthorityDistrict)
                    .Select(group => group.Key),
                Region = locations.GroupBy(composite => composite.Region).Select(group => group.Key)
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