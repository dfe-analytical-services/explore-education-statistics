using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class LocationRepository : AbstractRepository<Location, Guid>, ILocationRepository
    {
        public LocationRepository(StatisticsDbContext context) : base(context)
        {
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(Guid subjectId)
        {
            var observations = _context.Observation
                .Where(o => o.SubjectId == subjectId);
            return GetObservationalUnits(observations);
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var list = observations
                // NOTE: Not including Location to avoid join for performance
                .Select(o => new {o.GeographicLevel, o.LocationId})
                .AsNoTracking()
                .ToList();

            var geographicLevels = list
                .Select(elem => elem.GeographicLevel)
                .Distinct()
                .ToList();

            // NOTE: Get all locations in a single DB call
            var locationIds = list
                .Select(elem => elem.LocationId)
                .Distinct()
                .ToArray();
            var locationsDict = Find(locationIds)
                .AsNoTracking()
                .ToList()
                .ToDictionary(location => location.Id);

            return geographicLevels
                .ToDictionary(
                    geographicLevel => geographicLevel,
                    geographicLevel =>
                    {
                        var locations = list
                            .Where(elem => elem.GeographicLevel == geographicLevel)
                            .Select(elem => locationsDict[elem.LocationId])
                            .Distinct()
                            .ToList();
                        return locations
                            .Select(location => GetObservationalUnit(geographicLevel, location))
                            .Distinct();
                    });
        }

        public IEnumerable<IObservationalUnit> GetObservationalUnits(GeographicLevel level, IEnumerable<string> codes)
        {
            IQueryable<IObservationalUnit> query = level switch
            {
                GeographicLevel.EnglishDevolvedArea =>
                    _context.Location
                        .Where(q => codes.Contains(q.EnglishDevolvedArea_Code))
                        .GroupBy(q => new {q.EnglishDevolvedArea_Code, q.EnglishDevolvedArea_Name})
                        .Select(q => new EnglishDevolvedArea(q.Key.EnglishDevolvedArea_Code, q.Key.EnglishDevolvedArea_Name)),
                GeographicLevel.LocalAuthority =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalAuthority_Code))
                        .GroupBy(q => new {q.LocalAuthority_Code, q.LocalAuthority_OldCode, q.LocalAuthority_Name})
                        .Select(q => new LocalAuthority(q.Key.LocalAuthority_Code, q.Key.LocalAuthority_OldCode, q.Key.LocalAuthority_Name)),
                GeographicLevel.LocalAuthorityDistrict =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalAuthorityDistrict_Code))
                        .GroupBy(q => new {q.LocalAuthorityDistrict_Code, q.LocalAuthorityDistrict_Name})
                        .Select(q => new LocalAuthorityDistrict(q.Key.LocalAuthorityDistrict_Code, q.Key.LocalAuthorityDistrict_Name)),
                GeographicLevel.LocalEnterprisePartnership =>
                    _context.Location
                        .Where(q => codes.Contains(q.LocalEnterprisePartnership_Code))
                        .GroupBy(q => new {q.LocalEnterprisePartnership_Code, q.LocalEnterprisePartnership_Name})
                        .Select(q => new LocalEnterprisePartnership(q.Key.LocalEnterprisePartnership_Code, q.Key.LocalEnterprisePartnership_Name)),
                GeographicLevel.Institution =>
                    _context.Location
                        .Where(q => codes.Contains(q.Institution_Code))
                        .GroupBy(q => new {q.Institution_Code, q.Institution_Name})
                        .Select(q => new Institution(q.Key.Institution_Code, q.Key.Institution_Name)),
                GeographicLevel.MayoralCombinedAuthority =>
                    _context.Location
                        .Where(q => codes.Contains(q.MayoralCombinedAuthority_Code))
                        .GroupBy(q => new {q.MayoralCombinedAuthority_Code, q.MayoralCombinedAuthority_Name})
                        .Select(q => new MayoralCombinedAuthority(q.Key.MayoralCombinedAuthority_Code, q.Key.MayoralCombinedAuthority_Name)),
                GeographicLevel.MultiAcademyTrust =>
                    _context.Location
                        .Where(q => codes.Contains(q.MultiAcademyTrust_Code))
                        .GroupBy(q => new {q.MultiAcademyTrust_Code, q.MultiAcademyTrust_Name})
                        .Select(q => new Mat(q.Key.MultiAcademyTrust_Code, q.Key.MultiAcademyTrust_Name)),
                GeographicLevel.Country =>
                    _context.Location
                        .Where(q => codes.Contains(q.Country_Code))
                        .GroupBy(q => new {q.Country_Code, q.Country_Name})
                        .Select(q => new Country(q.Key.Country_Code, q.Key.Country_Name)),
                GeographicLevel.OpportunityArea =>
                    _context.Location
                        .Where(q => codes.Contains(q.OpportunityArea_Code))
                        .GroupBy(q => new {q.OpportunityArea_Code, q.OpportunityArea_Name})
                        .Select(q => new OpportunityArea(q.Key.OpportunityArea_Code, q.Key.OpportunityArea_Name)),
                GeographicLevel.ParliamentaryConstituency =>
                    _context.Location
                        .Where(q => codes.Contains(q.ParliamentaryConstituency_Code))
                        .GroupBy(q => new {q.ParliamentaryConstituency_Code, q.ParliamentaryConstituency_Name})
                        .Select(q => new ParliamentaryConstituency(q.Key.ParliamentaryConstituency_Code, q.Key.ParliamentaryConstituency_Name)),
                GeographicLevel.Provider =>
                    _context.Location
                        .Where(q => codes.Contains(q.Provider_Code))
                        .GroupBy(q => new {q.Provider_Code, q.Provider_Name})
                        .Select(q => new Provider(q.Key.Provider_Code, q.Key.Provider_Name)),
                GeographicLevel.Region =>
                    _context.Location
                        .Where(q => codes.Contains(q.Region_Code))
                        .GroupBy(q => new {q.Region_Code, q.Region_Name})
                        .Select(q => new Region(q.Key.Region_Code, q.Key.Region_Name)),
                GeographicLevel.RscRegion =>
                    _context.Location
                        .Where(q => codes.Contains(q.RscRegion_Code))
                        .GroupBy(q => new {q.RscRegion_Code})
                        .Select(q => new RscRegion(q.Key.RscRegion_Code)),
                GeographicLevel.School =>
                    _context.Location
                        .Where(q => codes.Contains(q.School_Code))
                        .GroupBy(q => new {q.School_Code, q.School_Name})
                        .Select(q => new School(q.Key.School_Code, q.Key.School_Name)),
                GeographicLevel.Sponsor =>
                    _context.Location
                        .Where(q => codes.Contains(q.Sponsor_Code))
                        .GroupBy(q => new {q.Sponsor_Code, q.Sponsor_Name})
                        .Select(q => new Sponsor(q.Key.Sponsor_Code, q.Key.Sponsor_Name)),
                GeographicLevel.Ward =>
                    _context.Location
                        .Where(q => codes.Contains(q.Ward_Code))
                        .GroupBy(q => new {q.Ward_Code, q.Ward_Name})
                        .Select(q => new Ward(q.Key.Ward_Code, q.Key.Ward_Name)),
                GeographicLevel.PlanningArea =>
                    _context.Location
                        .Where(q => codes.Contains(q.PlanningArea_Code))
                        .GroupBy(q => new {q.PlanningArea_Code, q.PlanningArea_Name})
                        .Select(q => new PlanningArea(q.Key.PlanningArea_Code, q.Key.PlanningArea_Name)),
                _ => throw new ArgumentOutOfRangeException()
            };

            return query.ToList();
        }

        private static IObservationalUnit GetObservationalUnit(GeographicLevel geographicLevel, Location location)
        {
            switch (geographicLevel)
            {
                case GeographicLevel.EnglishDevolvedArea:
                    return location.EnglishDevolvedArea;
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
                case GeographicLevel.Provider:
                    return location.Provider;
                case GeographicLevel.Region:
                    return location.Region;
                case GeographicLevel.RscRegion:
                    return location.RscRegion;
                case GeographicLevel.School:
                    return location.School;
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
    }
}
