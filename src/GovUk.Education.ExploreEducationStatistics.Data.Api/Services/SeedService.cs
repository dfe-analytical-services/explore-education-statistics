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
            return SeedGeoglevelsData();
        }

        private int SeedGeoglevelsData()
        {
            var mongoGeoLevelsCsvImporter = new MongoGeoLevelsCsvImporter();

            var count = 0;
            
            count += Seed(mongoGeoLevelsCsvImporter, Publication.absence, DataCsvFilename.absence_geoglevels);
            count += Seed(mongoGeoLevelsCsvImporter, Publication.exclusion, DataCsvFilename.exclusion_geoglevels);
            count += Seed(mongoGeoLevelsCsvImporter, Publication.schpupnum, DataCsvFilename.schpupnum_geoglevels);

            return count;
        }

        private int Seed(MongoGeoLevelsCsvImporter mongoGeoLevelsCsvImporter, Publication publication, DataCsvFilename dataCsvFilename)
        {
            var data = mongoGeoLevelsCsvImporter.Data(dataCsvFilename);
            Collection(publication).InsertMany(data);
            return data.Count;
        }
        
        private IMongoCollection<TidyData> Collection(Publication publication)
        {
            return _database.GetCollection<TidyData>(publication.ToString());
        }
    }
}