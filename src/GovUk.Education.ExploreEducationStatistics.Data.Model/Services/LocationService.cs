using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LocationService : AbstractRepository<Location, long>, ILocationService
    {
        public LocationService(ApplicationDbContext context, ILogger<LocationService> logger) : base(context, logger)
        {
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            SubjectMetaQueryContext query)
        {
            var locations = GetLocations(query);
            return GetObservationalUnits(locations);
        }

        public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IEnumerable<Location> locations)
        {
            return new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>
                {
                    {
                        GeographicLevel.National,
                        GroupByObservationalUnit(locations, location => location.Country)
                    },
                    {
                        GeographicLevel.Institution,
                        GroupByObservationalUnit(locations, location => location.Institution)
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
                        GeographicLevel.Regional,
                        GroupByObservationalUnit(locations, location => location.Region)
                    },
                    {
                        GeographicLevel.RSC_Region,
                        GroupByObservationalUnit(locations, location => location.RscRegion)
                    },
                    {
                        GeographicLevel.Ward,
                        GroupByObservationalUnit(locations, location => location.Ward)
                    }
                }
                .Where(pair => pair.Value.Any())
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private IEnumerable<Location> GetLocations(SubjectMetaQueryContext query)
        {
            var locationIds = _context.Observation
                .AsNoTracking()
                .Where(query.ObservationPredicate())
                .GroupBy(observation => observation.LocationId)
                .Select(group => group.Key).ToArray();

            if (locationIds.Any())
            {
                var locations = Find(locationIds).ToList();
                locations.ForEach(location => location.ReplaceEmptyOwnedTypeValuesWithNull());
                return locations;
            }

            return new List<Location>();
        }

        private static IEnumerable<T> GroupByObservationalUnit<T>(IEnumerable<Location> locations,
            Func<Location, T> keySelector) where T : IObservationalUnit
        {
            return locations.GroupBy(keySelector)
                .Where(grouping => grouping.Key != null)
                .Select(group => group.Key);
        }
    }
}