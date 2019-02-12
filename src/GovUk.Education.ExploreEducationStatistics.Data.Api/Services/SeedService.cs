using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<SeedConfigurationOptions> _options;

        private const string attributeMetaCollectionName = "AttributeMeta";
        private const string characteristicMetaCollectionName = "CharacteristicMeta";
        
        public SeedService(IConfiguration config,
            ILogger<SeedService> logger,
            IAzureDocumentService azureDocumentService,
            IOptions<SeedConfigurationOptions> options)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _logger = logger;
            _database = client.GetDatabase("education-statistics");
            _azureDocumentService = azureDocumentService;
            _options = options;
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
            await CreatePartitionedCollectionIfNotExists(attributeMetaCollectionName);
            await CreatePartitionedCollectionIfNotExists(characteristicMetaCollectionName);

            foreach (var publication in SamplePublications.Publications.Values)
            {
                await Seed(publication);
            }
        }

        private async Task Seed(Publication publication)
        {
            _logger.LogInformation("Seeding Publication {Publication}", publication.PublicationId);

            // seed publication attributes and characteristics
            await SeedAttributes(publication);
            await SeedCharacteristics(publication);

            // create collection if it does not exist
            await CreatePartitionedCollectionIfNotExists(publication.PublicationId.ToString());

            // get the collection
            var collection = _database.GetCollection<TidyData>(publication.PublicationId.ToString());

            if (collection.CountDocuments(data => data.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var release in publication.Releases)
                {
                    await SeedRelease(release, collection);
                }
            }
            else
            {
                _logger.LogWarning("Not seeding {Name}. Collection is not empty for publication {Publication}", publication.Name, publication.PublicationId);
            }
        }

        private async Task SeedRelease(Release release, IMongoCollection<TidyData> collection)
        {
            _logger.LogInformation("Seeding Publication {Publication}, Released {Release}", release.PublicationId,release.ReleaseId);

            var tasks = new List<Task>();

            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _csvImporterFactory.Importer(dataCsvFilename);
                
                var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,release.ReleaseDate);

                var batches = data.Batch(_options.Value.BatchSize);

                var i = 0;
                foreach (var batch in batches)
                {
                    await InsertManyAsync(collection, batch, i + 1, batches.Count(), release);
                    i++;
                }
            }
        }

        private async Task SeedAttributes(Publication publication)
        {
            _logger.LogInformation("Seeding {Collection}", attributeMetaCollectionName);

            var collection = _database.GetCollection<AttributeMeta>(attributeMetaCollectionName);

            if (collection.CountDocuments(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var attributeMeta in publication.AttributeMetas)
                {
                    attributeMeta.PublicationId = publication.PublicationId;
                }

                await collection.InsertManyAsync(publication.AttributeMetas);
            }

            _logger.LogWarning("Not seeding {Collection}. Collection is not empty for publication {Publication}",
                attributeMetaCollectionName, publication.PublicationId);
        }

        private async Task SeedCharacteristics(Publication publication)
        {
            _logger.LogInformation("Seeding {Collection}", characteristicMetaCollectionName);

            var collection = _database.GetCollection<CharacteristicMeta>(characteristicMetaCollectionName);

            if (collection.CountDocuments(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var characteristicMeta in publication.CharacteristicMetas)
                {
                    characteristicMeta.PublicationId = publication.PublicationId;
                }

                await collection.InsertManyAsync(publication.CharacteristicMetas);
            }

            _logger.LogWarning("Not seeding {Collection}. Collection is not empty for publication {Publication}",
                characteristicMetaCollectionName, publication.PublicationId);
        }

        private async Task InsertManyAsync(IMongoCollection<TidyData> collection, IEnumerable<TidyData> tidyData, int index,
            int totalCount, Release release)
        {
            _logger.LogInformation("Seeding batch {Index} of {TotalCount} for Publication {Publication}, Release {Release}", index,totalCount, release.PublicationId, release.ReleaseId);

            try
            {
                await collection.InsertManyAsync(tidyData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task CreatePartitionedCollectionIfNotExists(string name)
        {
            await _azureDocumentService.CreatePartitionedCollectionIfNotExists(name, "/'$v'/PublicationId/'$v'");
        }
    }
}