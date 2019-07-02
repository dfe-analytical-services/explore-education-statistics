using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterLocationService
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;

        public ImporterLocationService(IMemoryCache cache,
            ApplicationDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public Location Find(Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var cacheKey = GetCacheKey(country, institution, localAuthority, localAuthorityDistrict,
                localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                parliamentaryConstituency, region, rscRegion, sponsor, ward);

            if (_cache.TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(country, institution, localAuthority, localAuthorityDistrict,
                localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                parliamentaryConstituency, region, rscRegion, sponsor, ward);
            _cache.Set(cacheKey, location,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return location;
        }

        private static string GetCacheKey(Country country,
            Institution institution,
            LocalAuthority localAuthority,
            LocalAuthorityDistrict localAuthorityDistrict,
            LocalEnterprisePartnership localEnterprisePartnership,
            MayoralCombinedAuthority mayoralCombinedAuthority,
            Mat multiAcademyTrust,
            OpportunityArea opportunityArea,
            ParliamentaryConstituency parliamentaryConstituency,
            Region region,
            RscRegion rscRegion,
            Sponsor sponsor,
            Ward ward)
        {
            var observationalUnits = new IObservationalUnit[]
            {
                country, institution, localAuthority, localAuthorityDistrict, localEnterprisePartnership,
                mayoralCombinedAuthority, multiAcademyTrust, parliamentaryConstituency, opportunityArea, region,
                rscRegion, sponsor, ward
            };

            const string separator = "_";

            // TODO avoid possibility of a collision between different types of codes

            return string.Join(separator, observationalUnits
                .Where(unit => unit != null)
                .Select(unit => unit.Code));
        }

        private Location LookupOrCreate(Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var location = Lookup(
                country,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                opportunityArea,
                parliamentaryConstituency,
                region,
                rscRegion,
                sponsor,
                ward);

            if (location == null)
            {
                var entityEntry = _context.Location.Add(new Location
                {
                    Country = country ?? Country.Empty(),
                    Institution = institution ?? Institution.Empty(),
                    LocalAuthority = localAuthority ?? LocalAuthority.Empty(),
                    LocalAuthorityDistrict = localAuthorityDistrict ?? LocalAuthorityDistrict.Empty(),
                    LocalEnterprisePartnership = localEnterprisePartnership ?? LocalEnterprisePartnership.Empty(),
                    MayoralCombinedAuthority = mayoralCombinedAuthority ?? MayoralCombinedAuthority.Empty(),
                    MultiAcademyTrust = multiAcademyTrust ?? Mat.Empty(),
                    OpportunityArea = opportunityArea ?? OpportunityArea.Empty(),
                    ParliamentaryConstituency = parliamentaryConstituency ?? ParliamentaryConstituency.Empty(),
                    Region = region ?? Region.Empty(),
                    RscRegion = rscRegion ?? RscRegion.Empty(),
                    Ward = ward ?? Ward.Empty()
                });

                _context.SaveChanges();
                return entityEntry.Entity;
            }

            return location;
        }

        private Location Lookup(Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
                .And(location => location.Country.Code == country.Code);

            predicateBuilder = predicateBuilder.And(location =>
                location.Institution.Code ==
                (institution != null ? institution.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.LocalAuthority.Code ==
                (localAuthority != null ? localAuthority.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.LocalAuthorityDistrict.Code ==
                (localAuthorityDistrict != null ? localAuthorityDistrict.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.LocalEnterprisePartnership.Code ==
                (localEnterprisePartnership != null ? localEnterprisePartnership.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.MayoralCombinedAuthority.Code ==
                (mayoralCombinedAuthority != null ? mayoralCombinedAuthority.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.MultiAcademyTrust.Code ==
                (multiAcademyTrust != null ? multiAcademyTrust.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.OpportunityArea.Code ==
                (opportunityArea != null ? opportunityArea.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.ParliamentaryConstituency.Code ==
                (parliamentaryConstituency != null ? parliamentaryConstituency.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.Region.Code ==
                (region != null ? region.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.RscRegion.Code ==
                (rscRegion != null ? rscRegion.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.Sponsor.Code ==
                (sponsor != null ? sponsor.Code : null));

            predicateBuilder = predicateBuilder.And(location =>
                location.Ward.Code ==
                (ward != null ? ward.Code : null));

            return _context.Location.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}