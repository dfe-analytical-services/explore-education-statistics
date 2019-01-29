using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly ILogger _logger;
        private readonly IMongoDatabase _database;
        private readonly CsvImporterFactory _csvImporterFactory = new CsvImporterFactory();

        public SeedService(IConfiguration config, ILogger<SeedService> logger)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _database = client.GetDatabase("education-statistics");
            _logger = logger;
        }

        public void DropAllCollections()
        {
            _database.ListCollectionNames().ForEachAsync(collection => _database.DropCollection(collection));
        }

        public async Task<int> Seed()
        {
            var count = 0;
            foreach (var p in SamplePublications.Publications.Values)
            {
                count = count + await Seed(p);
            }
                
            return count;
        }

        private async Task<int> Seed(Publication publication)
        {
            var count = 0;

            await SeedAttribute(publication, publication.AttributeMetas);

            await SeedCharacteristics(publication, publication.CharacteristicMetas);

            foreach (var release in publication.Releases)
            {
                count = count + await SeedPublication(release);
            }
            
            return count; 
        }

        private async Task<int> SeedPublication(Release release)
        {
            var count = 0;
            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _csvImporterFactory.Importer(dataCsvFilename);
                var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,release.ReleaseDate);
                
                await _database.GetCollection<TidyData>(release.PublicationId.ToString()).InsertManyAsync(data);
                count += data.Count;
            }

            return count;
        }

        private async Task SeedAttribute(Publication publication, ICollection<AttributeMeta> attributeMetas)
        {
            _logger.LogInformation("Seeding AttributeMeta");

            try
            {
                var collection = _database.GetCollection<AttributeMeta>("AttributeMeta");
                
                foreach (var attributeMeta in attributeMetas)
                {
                    attributeMeta.PublicationId = publication.PublicationId;
                }

                await collection.InsertManyAsync(attributeMetas);
                
                _logger.LogInformation("Seeded " + attributeMetas.Count + " items in AttributeMeta");

            }
            catch (Exception e)
            {
                _logger.LogError("Unable to seed AttributeMeta");

                Console.WriteLine(e);
            }
        }

        private async Task SeedCharacteristics(Publication publication, ICollection<CharacteristicMeta> characteristicMetas)
        {
            _logger.LogInformation("Seeding CharacteristicMeta");
            
            try
            {
                foreach (var characteristicMeta in characteristicMetas)
                {
                    characteristicMeta.PublicationId = publication.PublicationId;
                }
                
                await _database.GetCollection<CharacteristicMeta>("CharacteristicMeta").InsertManyAsync(characteristicMetas);
                
                _logger.LogInformation("Seeded " + characteristicMetas.Count + " items in CharacteristicMeta");

            }
            catch (Exception e)
            {
                _logger.LogError("Unable to seed CharacteristicMeta");

                Console.WriteLine(e);
            }
        }
    }
}