using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LocationService : AbstractRepository<Location, Guid>, ILocationService
    {
        public static readonly List<GeographicLevel> IgnoredLevels = new List<GeographicLevel>
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

        public IEnumerable<IObservationalUnit> GetObservationalUnits(GeographicLevel level, IEnumerable<string> codes)
        {
            IQueryable<IObservationalUnit> query = level switch
            {
                GeographicLevel.LocalAuthority =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalAuthority.Code))
                        .GroupBy(q => new {q.LocalAuthority.Code, q.LocalAuthority.OldCode, q.LocalAuthority.Name})
                        .Select(q => new LocalAuthority(q.Key.Code, q.Key.OldCode, q.Key.Name)),
                GeographicLevel.LocalAuthorityDistrict =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalAuthorityDistrict.Code))
                        .GroupBy(q => new {q.LocalAuthorityDistrict.Code, q.LocalAuthorityDistrict.Name})
                        .Select(q => new LocalAuthorityDistrict(q.Key.Code, q.Key.Name)),
                GeographicLevel.LocalEnterprisePartnership =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalEnterprisePartnership.Code))
                        .GroupBy(q => new {q.LocalEnterprisePartnership.Code, q.LocalEnterprisePartnership.Name})
                        .Select(q => new LocalEnterprisePartnership(q.Key.Code, q.Key.Name)),
                GeographicLevel.Institution =>
                    _context.Location
                        .Where(q => codes.Contains(q.Institution.Code))
                        .GroupBy(q => new {q.Institution.Code, q.Institution.Name})
                        .Select(q => new Institution(q.Key.Code, q.Key.Name)),
                GeographicLevel.MayoralCombinedAuthority =>
                    _context.Location
                        .Where(q => codes.Contains(q.MayoralCombinedAuthority.Code))
                        .GroupBy(q => new {q.MayoralCombinedAuthority.Code, q.MayoralCombinedAuthority.Name})
                        .Select(q => new MayoralCombinedAuthority(q.Key.Code, q.Key.Name)),
                GeographicLevel.MultiAcademyTrust =>
                    _context.Location
                        .Where(q => codes.Contains(q.MultiAcademyTrust.Code))
                        .GroupBy(q => new {q.MultiAcademyTrust.Code, q.MultiAcademyTrust.Name})
                        .Select(q => new Mat(q.Key.Code, q.Key.Name)),
                GeographicLevel.Country =>
                    _context.Location
                        .Where(q => codes.Contains(q.Country.Code))
                        .GroupBy(q => new {q.Country.Code, q.Country.Name})
                        .Select(q => new Country(q.Key.Code, q.Key.Name)),
                GeographicLevel.OpportunityArea =>
                    _context.Location
                        .Where(q => codes.Contains(q.OpportunityArea.Code))
                        .GroupBy(q => new {q.OpportunityArea.Code, q.OpportunityArea.Name})
                        .Select(q => new OpportunityArea(q.Key.Code, q.Key.Name)),
                GeographicLevel.ParliamentaryConstituency =>
                    _context.Location
                        .Where(q => codes.Contains(q.ParliamentaryConstituency.Code))
                        .GroupBy(q => new {q.ParliamentaryConstituency.Code, q.ParliamentaryConstituency.Name})
                        .Select(q => new ParliamentaryConstituency(q.Key.Code, q.Key.Name)),
                GeographicLevel.Region =>
                    _context.Location
                        .Where(q => codes.Contains(q.Region.Code))
                        .GroupBy(q => new {q.Region.Code, q.Region.Name})
                        .Select(q => new Region(q.Key.Code, q.Key.Name)),
                GeographicLevel.RscRegion =>
                    _context.Location
                        .Where(q => codes.Contains(q.RscRegion.Code))
                        .GroupBy(q => new {q.RscRegion.Code})
                        .Select(q => new RscRegion(q.Key.Code)),
                GeographicLevel.Sponsor =>
                    _context.Location
                        .Where(q => codes.Contains(q.Sponsor.Code))
                        .GroupBy(q => new {q.Sponsor.Code, q.Sponsor.Name})
                        .Select(q => new Sponsor(q.Key.Code, q.Key.Name)),
                GeographicLevel.Ward =>
                    _context.Location
                        .Where(q => codes.Contains(q.Ward.Code))
                        .GroupBy(q => new {q.Ward.Code, q.Ward.Name})
                        .Select(q => new Ward(q.Key.Code, q.Key.Name)),
                GeographicLevel.PlanningArea =>
                    _context.Location
                        .Where(q => codes.Contains(q.PlanningArea.Code))
                        .GroupBy(q => new {q.PlanningArea.Code, q.PlanningArea.Name})
                        .Select(q => new PlanningArea(q.Key.Code, q.Key.Name)),
                _ => throw new ArgumentOutOfRangeException()
            };

            return query.ToList();
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
                case GeographicLevel.PlanningArea:
                    return location.PlanningArea;
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