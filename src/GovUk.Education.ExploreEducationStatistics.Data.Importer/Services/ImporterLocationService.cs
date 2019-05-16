using System;
using System.Linq;
using System.Text;
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
            Region region = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            Institution institution = null,
            Mat mat = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Ward ward = null)
        {
            var cacheKey = GetCacheKey(country, region, localAuthority, localAuthorityDistrict, localEnterprisePartnership,
                institution, mat, mayoralCombinedAuthority, opportunityArea, parliamentaryConstituency, provider, ward);

            if (_cache.TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(country, region, localAuthority, localAuthorityDistrict, localEnterprisePartnership,
                institution, mat, mayoralCombinedAuthority, opportunityArea, parliamentaryConstituency, provider, ward);
            _cache.Set(cacheKey, location,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return location;
        }

        private static string GetCacheKey(Country country,
            Region region,
            LocalAuthority localAuthority,
            LocalAuthorityDistrict localAuthorityDistrict,
            LocalEnterprisePartnership localEnterprisePartnership,
            Institution institution,
            Mat mat,
            MayoralCombinedAuthority mayoralCombinedAuthority,
            OpportunityArea opportunityArea,
            ParliamentaryConstituency parliamentaryConstituency,
            Provider provider,
            Ward ward)
        {
            const string separator = "_";

            var stringBuilder = new StringBuilder(country.Code);

            if (region != null)
            {
                stringBuilder.Append(separator).Append(region.Code);
            }

            // TODO avoid possibility of a collision between different types of codes

            if (localAuthority != null)
            {
                stringBuilder.Append(separator).Append(localAuthority.Code);
            }

            if (localAuthorityDistrict != null)
            {
                stringBuilder.Append(separator).Append(localAuthorityDistrict.Code);
            }

            if (localEnterprisePartnership != null)
            {
                stringBuilder.Append(separator).Append(localEnterprisePartnership.Code);
            }
            
            if (institution != null)
            {
                stringBuilder.Append(separator).Append(institution.Code);
            }
            
            if (mat != null)
            {
                stringBuilder.Append(separator).Append(mat.Code);
            }
            
            if (mayoralCombinedAuthority != null)
            {
                stringBuilder.Append(separator).Append(mayoralCombinedAuthority.Code);
            }
            
            if (parliamentaryConstituency != null)
            {
                stringBuilder.Append(separator).Append(parliamentaryConstituency.Code);
            }
            
            if (opportunityArea != null)
            {
                stringBuilder.Append(separator).Append(opportunityArea.Code);
            }
            
            if (provider != null)
            {
                stringBuilder.Append(separator).Append(provider.Code);
            }
            
            if (ward != null)
            {
                stringBuilder.Append(separator).Append(ward.Code);
            }
            return stringBuilder.ToString();
        }

        private Location LookupOrCreate(Country country,
            Region region = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            Institution institution = null,
            Mat mat = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Ward ward = null)
        {
            var location = Lookup(
                country,
                region,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                institution,
                mat,
                mayoralCombinedAuthority,
                opportunityArea,
                parliamentaryConstituency,
                provider,
                ward);

            if (location == null)
            {
                var entityEntry = _context.Location.Add(new Location
                {
                    Country = country ?? Country.Empty(),
                    Region = region ?? Region.Empty(),
                    LocalAuthority = localAuthority ?? LocalAuthority.Empty(),
                    LocalAuthorityDistrict = localAuthorityDistrict ?? LocalAuthorityDistrict.Empty(),
                    LocalEnterprisePartnership = localEnterprisePartnership ?? LocalEnterprisePartnership.Empty(),
                    Institution = institution ?? Institution.Empty(),
                    Mat = mat ?? Mat.Empty(),
                    MayoralCombinedAuthority = mayoralCombinedAuthority ?? MayoralCombinedAuthority.Empty(),
                    OpportunityArea = opportunityArea ?? OpportunityArea.Empty(),
                    ParliamentaryConstituency = parliamentaryConstituency ?? ParliamentaryConstituency.Empty(),
                    Provider = provider ?? Provider.Empty(),
                    Ward = ward ?? Ward.Empty()
                });

                _context.SaveChanges();
                return entityEntry.Entity;
            }

            return location;
        }

        private Location Lookup(Country country,
            Region region = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            Institution institution = null,
            Mat mat = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Ward ward = null)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
                .And(location => location.Country.Code == country.Code);

            if (region != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.Region.Code == region.Code);
            }

            if (localAuthority != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.LocalAuthority.Code == localAuthority.Code);
            }

            if (localAuthorityDistrict != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.LocalAuthorityDistrict.Code == localAuthorityDistrict.Code);
            }

            if (localEnterprisePartnership != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.LocalEnterprisePartnership.Code == localEnterprisePartnership.Code);
            }
            
            if (institution != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.Institution.Code == institution.Code);
            }
            
            if (mat != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.Mat.Code == mat.Code);
            }
            
            if (mayoralCombinedAuthority != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.MayoralCombinedAuthority.Code == mayoralCombinedAuthority.Code);
            }
            
            if (opportunityArea != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.OpportunityArea.Code == opportunityArea.Code);
            }
            
            if (parliamentaryConstituency != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.ParliamentaryConstituency.Code == parliamentaryConstituency.Code);
            }
            
            if (provider != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.Provider.Code == provider.Code);
            }
            
            if (ward != null)
            {
                predicateBuilder = predicateBuilder.And(location =>
                    location.Ward.Code == ward.Code);
            }
            return _context.Location.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}