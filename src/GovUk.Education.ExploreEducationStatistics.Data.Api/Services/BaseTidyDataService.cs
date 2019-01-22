using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public abstract class BaseTidyDataService<TCollection, TQueryContext> : ITidyDataService<TCollection, TQueryContext>
        where TCollection : TidyData
        where TQueryContext : IQueryContext<TCollection>
    {
        private readonly IMDatabase<TidyData> _database;

        protected BaseTidyDataService(IMDatabase<TidyData> database)
        {
            _database = database;
        }

        private IMongoCollection<TidyData> Collection(string collectionName)
        {
            return _database.Collection(collectionName);
        }

        private IMongoQueryable<TCollection> Queryable(string collectionName)
        {
            return Collection(collectionName).AsQueryable().OfType<TCollection>();
        }

        public IEnumerable<TCollection> FindMany(TQueryContext queryContext)
        {
            return Queryable(queryContext.PublicationId.ToString()).Where(queryContext.FindExpression()).ToList();
        }
    }
}