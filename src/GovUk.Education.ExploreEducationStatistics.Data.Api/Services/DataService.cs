using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    [Obsolete]
    public class DataService
    {
        private readonly MDatabase _database;

        public DataService(MDatabase database)
        {
            _database = database;
        }

        public IEnumerable<GeographicData> Get(string publication, string level = "")
        {
            return Queryable(publication).Where(x => x.Level == level).ToList();
        }

        private IMongoQueryable<GeographicData> Queryable(string collectionName)
        {
            return _database.Collection(collectionName).AsQueryable().OfType<GeographicData>();
        }
    }
}