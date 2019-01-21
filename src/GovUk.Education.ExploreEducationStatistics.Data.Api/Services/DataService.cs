using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataService
    {
        private readonly IMDatabase<TidyDataGeographic> _database;

        public DataService(IMDatabase<TidyDataGeographic> database)
        {
            _database = database;
        }

        public IEnumerable<TidyDataGeographic> Get(string publication, string level = "")
        {
            return _database.Collection(publication)
                                      .Find(x => x.Level == level)
                                      .ToList();
        }
    }
}