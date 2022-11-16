#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterLocationService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IImporterLocationCache _importerLocationCache;
        
        public ImporterLocationService(
            IGuidGenerator guidGenerator, 
            IImporterLocationCache importerLocationCache)
        {
            _guidGenerator = guidGenerator;
            _importerLocationCache = importerLocationCache;
        }
        
        public Location? Find(Location location)
        {
            return _importerLocationCache.Find(location);
        }

        public async Task<List<Location>> CreateIfNotExistsAndCache(
            StatisticsDbContext context, 
            List<Location> locations)
        {
            var results = await locations
                .ToAsyncEnumerable()
                .SelectAwait(async location =>
                    await _importerLocationCache
                        .GetOrCreateAndCacheAsync(location, async () =>
                        {
                            // Save and cache the new Location as soon as possible, as Locations are shareable between ongoing
                            // imports.  Therefore it is best to store it in the database as soon as possible so as to avoid 
                            // interfering with parallel imports of other Subjects using the same Locations.
                            location.Id = _guidGenerator.NewGuid();
                            var newLocation = (await context.AddAsync(location)).Entity;
                            return newLocation;
                        }))
                .ToListAsync();
            
            Console.WriteLine("Saving new Locations");
            await context.SaveChangesAsync();
            Console.WriteLine("Saved new Locations");
            return results;
        }

        public async Task<Location> CreateIfNotExistsAndCache(StatisticsDbContext context, Location location)
        {
            return (await CreateIfNotExistsAndCache(context, ListOf(location)))[0];
        }
    }
}
