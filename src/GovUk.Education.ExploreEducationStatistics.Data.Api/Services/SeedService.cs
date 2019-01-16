using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly IMongoDatabase _database;

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
            
            count += Seed(importer, Publication.absence, DataCsvFilename.absence_geoglevels);
            count += Seed(importer, Publication.exclusion, DataCsvFilename.exclusion_geoglevels);
            count += Seed(importer, Publication.schpupnum, DataCsvFilename.schpupnum_geoglevels);

            return count;
        }

        private int SeedNationalCharacteristicData()
        {
            var importer = new MongoNationalCharacteristicCsvImporter();

            var count = 0;
            
            count += Seed(importer, Publication.absence, DataCsvFilename.absence_natcharacteristics);
            count += Seed(importer, Publication.exclusion, DataCsvFilename.exclusion_natcharacteristics);
            count += Seed(importer, Publication.schpupnum, DataCsvFilename.schpupnum_natcharacteristics);

            return count;
        }

        private int SeedLaCharacteristicData()
        {
            var importer = new MongoLaCharacteristicCsvImporter();
            
            var count = 0;
            
            count += Seed(importer, Publication.absence, DataCsvFilename.absence_lacharacteristics);
            count += Seed(importer, Publication.exclusion, DataCsvFilename.exclusion_lacharacteristics);
            count += Seed(importer, Publication.schpupnum, DataCsvFilename.schpupnum_lacharacteristics);
            
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
            return _database.GetCollection<TidyData>(publication.ToString());
        }
    }
}