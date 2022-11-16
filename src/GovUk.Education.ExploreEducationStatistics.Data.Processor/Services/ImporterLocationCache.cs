#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.ImporterMemoryCache;
using DbUtils = GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterLocationCache : IImporterLocationCache
{
    private readonly Dictionary<string, Location> _locations = new();

    public ImporterLocationCache()
    {
        Console.WriteLine("Loading all Locations");
        var context = DbUtils.CreateStatisticsDbContext();
        
        var existingLocations = context
            .Location
            .AsNoTracking()
            .ToList();
        
        existingLocations.ForEach(location =>
        {
            var locationCacheKey = GetLocationCacheKey(location);
            // Console.WriteLine($"Adding existing Location {locationCacheKey}");
            _locations.Add(locationCacheKey, location);
        });
        Console.WriteLine("Finished loading all Locations");
    }

    public Location Find(Location locationFromCsv)
    {
        return _locations[GetLocationCacheKey(locationFromCsv)];
    }
    
    public async Task<Location> GetOrCreateAndCacheAsync(Location locationFromCsv, Func<Task<Location>> locationProvider)
    {
        var locationCacheKey = GetLocationCacheKey(locationFromCsv);

        if (_locations.ContainsKey(locationCacheKey))
        {
            Console.WriteLine($"Found existing Location {locationCacheKey}");
            return _locations[locationCacheKey];
        }

        var providedLocation = await locationProvider.Invoke();

        Console.WriteLine($"Created new Location {locationCacheKey}");

        _locations.Add(locationCacheKey, providedLocation);
        return providedLocation;
    }
}