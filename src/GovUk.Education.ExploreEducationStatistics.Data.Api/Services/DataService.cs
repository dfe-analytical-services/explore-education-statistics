using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.AspNetCore.Mvc;
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

        public IEnumerable<TableToolData> GetTableToolData()
        {
            var query = _database.GetCollection<TidyDataGeographic>("absence")
                .Find(x => x.Level == "national" && x.SchoolType == SchoolType.Total.ToString())
                .ToList();

            var mappedResults = query.Select(item => new TableToolData
            {
                Domain = item.Year.ToString(),
                Range = item.Attributes.Where(pair => pair.Key.Contains("_exact") || pair.Key.Contains("_percent"))
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
            });
            
            return mappedResults;
        }
    }
}