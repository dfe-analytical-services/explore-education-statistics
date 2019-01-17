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

        private readonly Dictionary<string, Publication> _publications = new Dictionary<string, Publication>
        {
            {"absence", new Publication {Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Title = "absence"}},
            {"exclusion", new Publication {Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), Title = "exclusion"}},
            {"schpupnum", new Publication {Id = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), Title = "schpupnum"}}
        };

        public SeedService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _database = client.GetDatabase("education-statistics");
        }

        public int Seed()
        {
            var count = 0;

            count += SeedGeoglevelsData();
            count += SeedNationalCharacteristicData();
            count += SeedLaCharacteristicData();

            return count;
        }

        private int SeedGeoglevelsData()
        {
            var importer = new MongoGeoLevelsCsvImporter();

            var count = 0;

            count += Seed(importer, _publications["absence"], DataCsvFilename.absence_geoglevels);
            count += Seed(importer, _publications["exclusion"], DataCsvFilename.exclusion_geoglevels);
            count += Seed(importer, _publications["schpupnum"], DataCsvFilename.schpupnum_geoglevels);

            return count;
        }

        private int SeedNationalCharacteristicData()
        {
            var importer = new MongoNationalCharacteristicCsvImporter();

            var count = 0;

            count += Seed(importer, _publications["absence"], DataCsvFilename.absence_natcharacteristics);
            count += Seed(importer, _publications["exclusion"], DataCsvFilename.exclusion_natcharacteristics);
            count += Seed(importer, _publications["schpupnum"], DataCsvFilename.schpupnum_natcharacteristics);

            return count;
        }

        private int SeedLaCharacteristicData()
        {
            var importer = new MongoLaCharacteristicCsvImporter();

            var count = 0;

            count += Seed(importer, _publications["absence"], DataCsvFilename.absence_lacharacteristics);
            count += Seed(importer, _publications["exclusion"], DataCsvFilename.exclusion_lacharacteristics);
            count += Seed(importer, _publications["schpupnum"], DataCsvFilename.schpupnum_lacharacteristics);

            return count;
        }

        private int Seed(IMongoCsvImporter mongoCsvImporter, Publication publication, DataCsvFilename dataCsvFilename)
        {
            var data = mongoCsvImporter.Data(dataCsvFilename);
            Collection(publication).InsertMany(data);
            return data.Count;
        }

        private IMongoCollection<TidyData> Collection(Publication publication)
        {
            return _database.GetCollection<TidyData>(publication.Id.ToString());
        }
    }
}