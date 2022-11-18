#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterLocationService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IImporterLocationCache _importerLocationCache;
        private readonly ILogger<ImporterLocationCache> _logger;

        public ImporterLocationService(
            IGuidGenerator guidGenerator, 
            IImporterLocationCache importerLocationCache, 
            ILogger<ImporterLocationCache> logger)
        {
            _guidGenerator = guidGenerator;
            _importerLocationCache = importerLocationCache;
            _logger = logger;
        }
        
        public Location Get(Location location)
        {
            return _importerLocationCache.Get(location);
        }

        public async Task<List<Location>> CreateIfNotExistsAndCache(
            StatisticsDbContext context, 
            List<Location> locationsToCheck)
        {
            var newLocationsCount = 0;
            
            var cachedLocations = await locationsToCheck
                .ToAsyncEnumerable()
                .SelectAwait(async location =>
                    await _importerLocationCache
                        .GetOrCreateAndCache(location, async () =>
                        {
                            // Save and cache the new Location as soon as possible, as Locations are shareable between
                            // ongoing imports.  Therefore it is best to track it for database entry and cache it as
                            // soon as possible so as to avoid interfering with parallel imports of other Subjects
                            // using the same Locations.
                            location.Id = _guidGenerator.NewGuid();
                            var newLocation = (await context.AddAsync(location)).Entity;
                            newLocationsCount++;
                            return newLocation;
                        }))
                .ToListAsync();

            if (newLocationsCount > 0)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation($"Added {newLocationsCount} new Locations");
            }
            
            return cachedLocations;
        }

        public async Task<Location> CreateIfNotExistsAndCache(StatisticsDbContext context, Location location)
        {
            return (await CreateIfNotExistsAndCache(context, ListOf(location))).Single();
        }
    }
}
