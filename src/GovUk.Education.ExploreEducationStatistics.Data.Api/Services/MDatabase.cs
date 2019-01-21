using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MDatabase<T> : IMDatabase<T>
    {
        public IMongoDatabase Database { get; }

        public MDatabase(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            Database = client.GetDatabase("education-statistics");
        }

        public IMongoCollection<T> Collection(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }
    }
}