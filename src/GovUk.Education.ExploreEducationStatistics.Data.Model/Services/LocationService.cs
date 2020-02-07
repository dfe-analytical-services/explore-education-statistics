using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LocationService : AbstractRepository<Location, Guid>, ILocationService
    {
        private static readonly List<GeographicLevel> IgnoredLevels = new List<GeographicLevel>
        {
            GeographicLevel.School,
            GeographicLevel.Provider
        };

        public LocationService(StatisticsDbContext context, ILogger<LocationService> logger) : base(context, logger)
        {
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(Guid subjectId)
        {
            var locations = GetLocationsGroupedByGeographicLevel(subjectId);
            return GetObservationalUnits(locations);
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var locations = GetLocationsGroupedByGeographicLevel(observations);
            return GetObservationalUnits(locations);
        }

        private static Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            Dictionary<GeographicLevel, IEnumerable<Location>> locations)
        {
            return locations.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(location => GetObservationalUnit(pair.Key, location))
            );
        }

        private static IObservationalUnit GetObservationalUnit(GeographicLevel geographicLevel, Location location)
        {
            switch (geographicLevel)
            {
                case GeographicLevel.LocalAuthority:
                    return location.LocalAuthority;
                case GeographicLevel.LocalAuthorityDistrict:
                    return location.LocalAuthorityDistrict;
                case GeographicLevel.LocalEnterprisePartnership:
                    return location.LocalEnterprisePartnership;
                case GeographicLevel.Institution:
                    return location.Institution;
                case GeographicLevel.MayoralCombinedAuthority:
                    return location.MayoralCombinedAuthority;
                case GeographicLevel.MultiAcademyTrust:
                    return location.MultiAcademyTrust;
                case GeographicLevel.Country:
                    return location.Country;
                case GeographicLevel.OpportunityArea:
                    return location.OpportunityArea;
                case GeographicLevel.ParliamentaryConstituency:
                    return location.ParliamentaryConstituency;
                case GeographicLevel.Region:
                    return location.Region;
                case GeographicLevel.RscRegion:
                    return location.RscRegion;
                case GeographicLevel.Sponsor:
                    return location.Sponsor;
                case GeographicLevel.Ward:
                    return location.Ward;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Dictionary<GeographicLevel, IEnumerable<Location>> GetLocationsGroupedByGeographicLevel(Guid subjectId)
        {
            var locationIdsWithGeographicLevel = _context.Observation
                .Where(observation =>
                    !IgnoredLevels.Contains(observation.GeographicLevel) && observation.SubjectId == subjectId)
                .Select(observation => new {observation.GeographicLevel, observation.LocationId})
                .Distinct()
                .ToList()
                .Select(arg => (arg.GeographicLevel, arg.LocationId));
            
            return GetLocationsGroupedByGeographicLevel(locationIdsWithGeographicLevel);
        }

        private Dictionary<GeographicLevel, IEnumerable<Location>> GetLocationsGroupedByGeographicLevel(
            IQueryable<Observation> observations)
        {
            var locationIdsWithGeographicLevel = observations
                .Where(observation => !IgnoredLevels.Contains(observation.GeographicLevel))
                .Select(observation => new {observation.GeographicLevel, observation.LocationId})
                .Distinct()
                .ToList()
                .Select(arg => (arg.GeographicLevel, arg.LocationId));
            
            return GetLocationsGroupedByGeographicLevel(locationIdsWithGeographicLevel);
        }

        private Dictionary<GeographicLevel, IEnumerable<Location>> GetLocationsGroupedByGeographicLevel(
            IEnumerable<(GeographicLevel GeographicLevel, Guid LocationId)> locationIdsWithGeographicLevel)
        {
            var locationIdsGroupedByGeographicLevel = locationIdsWithGeographicLevel.GroupBy(
                tuple => tuple.GeographicLevel,
                tuple => tuple.LocationId);

            var locations = GetLocations(locationIdsWithGeographicLevel.Select(arg => arg.LocationId).ToArray());

            return locationIdsGroupedByGeographicLevel
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.ToList().Select(id => locations[id]));
        }

        private Dictionary<Guid, Location> GetLocations(Guid[] locationIds)
        {
            var locations = Find(locationIds).ToList();
            return locations.ToDictionary(location => location.Id);
        }
    }
}