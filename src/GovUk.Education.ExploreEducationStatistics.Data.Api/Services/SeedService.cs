using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly IMongoDatabase _database;
        private readonly CsvImporterFactory _csvImporterFactory = new CsvImporterFactory();

        public SeedService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _database = client.GetDatabase("education-statistics");
        }

        public bool CanSeed()
        {
            return !_database.ListCollectionNames().Any();
        }

        public void DropAllCollections()
        {
            _database.ListCollectionNames().ForEachAsync(collection => _database.DropCollection(collection));
        }

        public int Seed()
        {
            return SamplePublications.Publications.Values.Sum(Seed);
        }

        private int Seed(Publication publication)
        {
            Seed(publication, publication.AttributeMetas);
            Seed(publication, publication.CharacteristicMetas);
            return publication.Releases.Sum(Seed);
        }

        private int Seed(Release release)
        {
            var count = 0;
            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _csvImporterFactory.Importer(dataCsvFilename);
                var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,
                    release.ReleaseDate);
                _database.GetCollection<TidyData>(release.PublicationId.ToString()).InsertManyAsync(data);
                count += data.Count;
            }

            return count;
        }

        private void Seed(Publication publication, AttributeMeta[] attributeMetas)
        {
            foreach (var attributeMeta in attributeMetas)
            {
                attributeMeta.PublicationId = publication.PublicationId;
            }

            _database.GetCollection<AttributeMeta>("AttributeMeta").InsertManyAsync(attributeMetas);
        }

        private void Seed(Publication publication, CharacteristicMeta[] characteristicMetas)
        {
            foreach (var characteristicMeta in characteristicMetas)
            {
                characteristicMeta.PublicationId = publication.PublicationId;
            }
            _database.GetCollection<CharacteristicMeta>("CharacteristicMeta").InsertManyAsync(characteristicMetas);
        }
    }
}