using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataService
    {
        private readonly IMongoCollection<TidyData> _collection;

        public DataService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("StatisticsDb"));
            var database = client.GetDatabase("education-statistics");
            _collection = database.GetCollection<TidyData>("absence");
        }
        
        public List<TidyData> Get()
        {
            // TODO: Temp limit on query
            var query = _collection.Find(x => x.Level == "national" && x.SchoolType == "Total").ToList();

            return query;
        }

        public TidyData Get(string id)
        {
            var docId = new ObjectId(id);

            return _collection.Find<TidyData>(book => book.Id == docId).FirstOrDefault();
        }

        public TidyData Create(TidyData book)
        {
            _collection.InsertOne(book);
            return book;
        }

        public void Update(string id, TidyData bookIn)
        {
            var docId = new ObjectId(id);

            _collection.ReplaceOne(book => book.Id == docId, bookIn);
        }

        public void Remove(TidyData bookIn)
        {
            _collection.DeleteOne(book => book.Id == bookIn.Id);
        }

        public void Remove(ObjectId id)
        {
            _collection.DeleteOne(book => book.Id == id);
        }
        
        public int Seed()
        {
            if (_collection.Find(x => x.Level == "national" && x.SchoolType == "Total").ToList().Any() == false)
            {
                // TODO: temp seed of data
                var seed = new MongoCsvImporter().GeoLevels("absence_geoglevels").ToList();

                _collection.InsertMany(seed);

                return seed.Count();
            }

            return 0;
        }
    }
}