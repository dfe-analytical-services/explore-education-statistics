using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly IMongoDatabase _database;
        private readonly MongoCsvImporterFactory _mongoCsvImporterFactory = new MongoCsvImporterFactory();

        private readonly Dictionary<string, Release[]> _publications = new Dictionary<string, Release[]>
        {
            {
                "absence", new[]
                {
                    new Release
                    {
                        PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                        ReleaseId = new Guid("1d395b31-a68e-489c-a257-b3ab5c40bb01"),
                        ReleaseDate = new DateTime(2018, 4, 25),
                        Filenames = new[]
                        {
                            DataCsvFilename.absence_geoglevels,
                            DataCsvFilename.absence_lacharacteristics,
                            DataCsvFilename.absence_natcharacteristics
                        },
                        Title = "absence"
                    }
                }
            },

            {
                "exclusion", new[]
                {
                    new Release
                    {
                        PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                        ReleaseId = new Guid("ac602576-2d07-4324-8480-0cabb6294814"),
                        ReleaseDate = new DateTime(2018, 3, 22),
                        Filenames = new[]
                        {
                            DataCsvFilename.exclusion_geoglevels,
                            DataCsvFilename.exclusion_lacharacteristics,
                            DataCsvFilename.exclusion_natcharacteristics
                        },
                        Title = "exclusion"
                    }
                }
            },

            {
                "schpupnum", new[]
                {
                    new Release
                    {
                        PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                        ReleaseId = new Guid("be51f939-e9f9-4509-8851-e72b66a3515b"),
                        ReleaseDate = new DateTime(2018, 5, 30),
                        Filenames = new[]
                        {
                            DataCsvFilename.schpupnum_geoglevels,
                            DataCsvFilename.schpupnum_lacharacteristics,
                            DataCsvFilename.schpupnum_natcharacteristics
                        },
                        Title = "schpupnum"
                    }
                }
            }
        };

        public SeedService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _database = client.GetDatabase("education-statistics");
        }

        public int Seed()
        {
            var count = 0;

            foreach (var publication in _publications.Keys)
            {
                foreach (var release in _publications[publication])
                {
                    count += Seed(release);
                }
            }

            return count;
        }

        public bool CanSeed()
        {
            return !_database.ListCollectionNames().Any();
        }

        public void DropAllCollections()
        {
            _database.ListCollectionNames().ForEachAsync(collection => _database.DropCollection(collection));
        }

        private int Seed(Release release)
        {
            var count = 0;
            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _mongoCsvImporterFactory.Importer(dataCsvFilename);
                var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,
                    release.ReleaseDate);
                Collection(release).InsertManyAsync(data);
                count += data.Count;
            }

            return count;
        }

        private IMongoCollection<TidyData> Collection(Release release)
        {
            return _database.GetCollection<TidyData>(release.PublicationId.ToString());
        }
    }
}