using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataService
    {
        private readonly IMongoDatabase _database;

        public DataService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            _database = client.GetDatabase("education-statistics");
        }

        public IEnumerable<TidyDataGeographic> Get(string publication, string level = "")
        {
            var query = _database.GetCollection<TidyDataGeographic>(publication)
                                      .Find(x => x.Level == level)
                                      .ToList();

            return query;
        }

//        public TidyDataGeographic GetLaEstab(string laEstab)
//        {
//            return _database.Find(elem => elem.School.LaEstab == laEstab).FirstOrDefault();
//        }
    }
}