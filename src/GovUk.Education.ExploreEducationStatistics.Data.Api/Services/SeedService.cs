using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly ILogger _logger;
        private readonly IMongoDatabase _database;
        private readonly IAzureDocumentService _azureDocumentService;
        private readonly CsvImporterFactory _csvImporterFactory = new CsvImporterFactory();

        private const string attributeMetaCollectionName = "AttributeMeta";
        private const string characteristicMetaCollectionName = "CharacteristicMeta";

        public SeedService(IConfiguration config, ILogger<SeedService> logger,
            IAzureDocumentService azureDocumentService)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _logger = logger;
            _database = client.GetDatabase("education-statistics");
            _azureDocumentService = azureDocumentService;
        }

        public async Task<long> EmptyAllCollectionsExceptMeta()
        {
            long count = 0;

            var collectionsToExclude = new[] {attributeMetaCollectionName, characteristicMetaCollectionName};

            await _database.ListCollectionNames().ForEachAsync(async name =>
            {
                if (!collectionsToExclude.Contains(name))
                {
                    var tasks = new List<Task<DeleteResult>>();
                    try
                    {
                        tasks.Add(DeleteDocumentsFromPublicationCollection(name));
                        var deleteResults = await Task.WhenAll(tasks);
                        foreach (var deleteResult in deleteResults)
                        {
                            count += deleteResult.DeletedCount;
                            _logger.LogInformation("Deleted {Count}", deleteResult.DeletedCount);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Caught error deleting documents from collection", e);
                    }
                }
            });

            return count;
        }

        private Task<DeleteResult> DeleteDocumentsFromPublicationCollection(string publicationId)
        {
            var collection = _database.GetCollection<BsonDocument>(publicationId);
            return collection.DeleteManyAsync(
                Builders<BsonDocument>.Filter.Eq("PublicationId", Guid.Parse(publicationId)));
        }

        public async void Seed()
        {
            CreatePartitionedCollectionIfNotExists(attributeMetaCollectionName);
            CreatePartitionedCollectionIfNotExists(characteristicMetaCollectionName);

            await Task.WhenAll(SamplePublications.Publications.Values.SelectMany(Seed));
        }

        private IEnumerable<Task> Seed(Publication publication)
        {
            _logger.LogInformation("Seeding Publication {Publication}", publication.PublicationId);

            var tasks = new List<Task> {SeedAttributes(publication), SeedCharacteristics(publication)};

            CreatePartitionedCollectionIfNotExists(publication.PublicationId.ToString());

            var collection = _database.GetCollection<TidyData>(publication.PublicationId.ToString());

            if (collection.CountDocuments(data => data.PublicationId == publication.PublicationId) == 0)
            {
                tasks.AddRange(publication.Releases.SelectMany(release => SeedRelease(release, collection)));
            }

            _logger.LogWarning("Not seeding {Name}. Collection is not empty for publication {Publication}",
                publication.Name, publication.PublicationId);

            return tasks;
        }

        private IEnumerable<Task> SeedRelease(Release release, IMongoCollection<TidyData> collection)
        {
            _logger.LogInformation("Seeding Publication {Publication}, Released {Release}", release.PublicationId,
                release.ReleaseId);

            var tasks = new List<Task>();

            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _csvImporterFactory.Importer(dataCsvFilename);
                var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,
                    release.ReleaseDate);

                var batches = data.Batch(100000);
                tasks.AddRange(batches
                    .Select((batch, i) => InsertManyAsync(collection, batch, i + 1, batches.Count(), release))
                    .ToList());
            }

            return tasks;
        }

        private Task SeedAttributes(Publication publication)
        {
            _logger.LogInformation("Seeding {Collection}", attributeMetaCollectionName);

            var collection = _database.GetCollection<AttributeMeta>(attributeMetaCollectionName);

            if (collection.CountDocuments(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var attributeMeta in publication.AttributeMetas)
                {
                    attributeMeta.PublicationId = publication.PublicationId;
                }

                return collection.InsertManyAsync(publication.AttributeMetas);
            }

            _logger.LogWarning("Not seeding {Collection}. Collection is not empty for publication {Publication}",
                attributeMetaCollectionName, publication.PublicationId);

            return Task.CompletedTask;
        }

        private Task SeedCharacteristics(Publication publication)
        {
            _logger.LogInformation("Seeding {Collection}", characteristicMetaCollectionName);

            var collection = _database.GetCollection<CharacteristicMeta>(characteristicMetaCollectionName);

            if (collection.CountDocuments(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var characteristicMeta in publication.CharacteristicMetas)
                {
                    characteristicMeta.PublicationId = publication.PublicationId;
                }

                return collection.InsertManyAsync(publication.CharacteristicMetas);
            }

            _logger.LogWarning("Not seeding {Collection}. Collection is not empty for publication {Publication}",
                characteristicMetaCollectionName, publication.PublicationId);

            return Task.CompletedTask;
        }

        private Task InsertManyAsync(IMongoCollection<TidyData> collection, IEnumerable<TidyData> tidyData, int index,
            int totalCount, Release release)
        {
            _logger.LogInformation(
                "Seeding batch {Index} of {TotalCount} for Publication {Publication}, Release {Release}", index,
                totalCount, release.PublicationId, release.ReleaseId);

            return collection.InsertManyAsync(tidyData);
        }

        private void CreatePartitionedCollectionIfNotExists(string name)
        {
            _azureDocumentService.CreatePartitionedCollectionIfNotExists(name, "/'$v'/PublicationId/'$v'");
        }
    }
}