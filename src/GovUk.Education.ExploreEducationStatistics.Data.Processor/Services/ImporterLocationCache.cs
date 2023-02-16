#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterLocationCache : IImporterLocationCache
{
    private readonly ConcurrentDictionary<string, Location> _locations = new();
    
    private readonly ILogger<ImporterLocationCache> _logger;

    public ImporterLocationCache(ILogger<ImporterLocationCache> logger)
    {
        _logger = logger;
    }

    public void LoadLocations(StatisticsDbContext context)
    {
        _logger.LogInformation("Loading all Locations into cache");
        
        var existingLocations = context
            .Location
            .AsNoTracking()
            .ToList();
        
        existingLocations.ForEach(location =>
        {
            var locationCacheKey = GetLocationCacheKey(location);
            
            var added = _locations.TryAdd(locationCacheKey, location);

            if (!added)
            {
                _logger.LogError("Duplicate Location has already been added to the Locations cache, indicating " +
                                 $"a duplicate Location in the environment's database - {locationCacheKey}");
            }
        });
        
        _logger.LogInformation($"Loaded {_locations.Count} Locations into cache successfully");
    }

    public Location Get(Location locationFromCsv)
    {
        return _locations[GetLocationCacheKey(locationFromCsv)];
    }
    
    public async Task<Location> GetOrCreateAndCache(Location locationFromCsv, Func<Task<Location>> locationProvider)
    {
        var locationCacheKey = GetLocationCacheKey(locationFromCsv);

        // Check for the existence of the location in the cache already. If it exists, return the cached version.
        if (_locations.ContainsKey(locationCacheKey))
        {
            return _locations[locationCacheKey];
        }

        // Otherwise, get the new Location from the "locationProvider" and add it to the cache.
        //
        // Note that we can't use ConcurrentDictionary's GetOrAdd() method to more cleanly write all of this, as it is
        // not thread-safe. Instead we manually invoke "locationProvider" to get a new Location and then use TryAdd()
        // which is a thread-safe way to add to the dictionary, and ignore collisions in the rare scenario when 2
        // import processes are trying to add the same Location at the same time.  
        var providedLocation = await locationProvider.Invoke();

        var added = _locations.TryAdd(locationCacheKey, providedLocation);

        // Due to the way that concurrent import processes take it in turns (via an exclusive lock) to add any new 
        // Locations they need to add to the database (and this cache), we should never run into a scenario whereby
        // 2 or more import processes attempt to add the same Location at any point. 
        if (!added)
        {
            throw new ArgumentException($"Location already added to cache - {locationCacheKey}");
        }
        
        return providedLocation;
    }

    private static string GetLocationCacheKey(
        GeographicLevel geographicLevel,
        Country? country,
        EnglishDevolvedArea? englishDevolvedArea,
        Institution? institution,
        LocalAuthority? localAuthority,
        LocalAuthorityDistrict? localAuthorityDistrict,
        LocalEnterprisePartnership? localEnterprisePartnership,
        MayoralCombinedAuthority? mayoralCombinedAuthority,
        MultiAcademyTrust? multiAcademyTrust,
        OpportunityArea? opportunityArea,
        ParliamentaryConstituency? parliamentaryConstituency,
        PlanningArea? planningArea,
        Provider? provider,
        Region? region,
        RscRegion? rscRegion,
        School? school,
        Sponsor? sponsor,
        Ward? ward)
    {
        var locationAttributes = new LocationAttribute?[]
        {
            country,
            englishDevolvedArea,
            institution,
            localAuthority,
            localAuthorityDistrict,
            localEnterprisePartnership,
            mayoralCombinedAuthority,
            multiAcademyTrust,
            parliamentaryConstituency,
            planningArea,
            provider,
            opportunityArea,
            region,
            rscRegion,
            school,
            sponsor,
            ward
        };

        var tokens = locationAttributes
            .WhereNotNull()
            .Select(attribute => attribute.GetCacheKey())
            .ToList();

        const char separator = '_';
        return $"{geographicLevel}{separator}{tokens.JoinToString(separator)}";
    }

    private static string GetLocationCacheKey(Location location)
    {
        return GetLocationCacheKey(
            location.GeographicLevel,
            location.Country,
            location.EnglishDevolvedArea,
            location.Institution,
            location.LocalAuthority,
            location.LocalAuthorityDistrict,
            location.LocalEnterprisePartnership,
            location.MayoralCombinedAuthority,
            location.MultiAcademyTrust,
            location.OpportunityArea,
            location.ParliamentaryConstituency,
            location.PlanningArea,
            location.Provider,
            location.Region,
            location.RscRegion,
            location.School,
            location.Sponsor,
            location.Ward);
    }
}