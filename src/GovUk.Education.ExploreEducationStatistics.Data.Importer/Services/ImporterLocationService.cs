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
            LocalAuthorityDistrict localAuthorityDistrict = null)
        {
            var cacheKey = GetCacheKey(country, region, localAuthority, localAuthorityDistrict);

            if (_cache.TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(country, region, localAuthority);
            _cache.Set(cacheKey, location,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return location;
        }

        private static string GetCacheKey(Country country,
            Region region,
            LocalAuthority localAuthority,
            LocalAuthorityDistrict localAuthorityDistrict)
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

            return stringBuilder.ToString();
        }

        private Location LookupOrCreate(Country country,
            Region region = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null)
        {
            var location = Lookup(country, region, localAuthority, localAuthorityDistrict);

            if (location == null)
            {
                var entityEntry = _context.Location.Add(new Location
                {
                    Country = country ?? Country.Empty(),
                    Region = region ?? Region.Empty(),
                    LocalAuthority = localAuthority ?? LocalAuthority.Empty(),
                    LocalAuthorityDistrict = localAuthorityDistrict ?? LocalAuthorityDistrict.Empty()
                });

                _context.SaveChanges();
                return entityEntry.Entity;
            }

            return location;
        }

        private Location Lookup(Country country,
            Region region = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null)
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

            return _context.Location.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}