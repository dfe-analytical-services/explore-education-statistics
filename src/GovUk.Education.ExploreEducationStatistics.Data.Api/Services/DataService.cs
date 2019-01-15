using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataService
    {
        private readonly IMongoCollection<TidyDataGeographic> _collection;

        public DataService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            var database = client.GetDatabase("education-statistics");
            _collection = database.GetCollection<TidyDataGeographic>("absence");
        }

        public List<TidyDataGeographic> Get()
        {
            // TODO: Temp limit on query
            var query = _collection.Find(x => x.Level == "national" && x.SchoolType == "Total").ToList();

            return query;
        }

        public TidyDataGeographic Get(string laEstab)
        {
            return _collection.Find(elem => elem.School.LaEstab == laEstab).FirstOrDefault();
        }
    }
}