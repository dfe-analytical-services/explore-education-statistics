using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MDatabase

    {
        public IMongoDatabase Database { get; }

        public MDatabase(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            Database = client.GetDatabase("education-statistics");
        }

        public IMongoCollection<TidyData> Collection(string collectionName)
        {
            return Database.GetCollection<TidyData>(collectionName);
        }
    }
}