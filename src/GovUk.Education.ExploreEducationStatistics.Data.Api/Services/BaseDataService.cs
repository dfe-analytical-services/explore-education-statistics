using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public abstract class BaseDataService<TCollection, TQueryContext> : IDataService<TCollection, TQueryContext>
        where TCollection : ITidyData
        where TQueryContext : IQueryContext<TCollection>
    {
        private readonly MDatabase _database;

        protected BaseDataService(MDatabase database)
        {
            _database = database;
        }

        private IMongoCollection<TidyData> Collection(TQueryContext queryContext)
        {
            return _database.Collection(queryContext.PublicationId.ToString());
        }

        private IMongoQueryable<TCollection> Queryable(TQueryContext queryContext)
        {
            return Collection(queryContext).AsQueryable().OfType<TCollection>();
        }

        public IEnumerable<TCollection> FindMany(TQueryContext queryContext)
        {
            var mostRecentReleaseId = FindMostRecentReleaseId(queryContext);
            
            return Queryable(queryContext)
                .Where(collection => collection.ReleaseId == mostRecentReleaseId)
                .Where(queryContext.FindExpression())
                .ToList();
        }

        private Guid FindMostRecentReleaseId(TQueryContext queryContext)
        {
            var options = new FindOptions<TidyData, TidyData>
            {
                Limit = 1,
                Sort = Builders<TidyData>.Sort.Descending(o => o.ReleaseDate)
            };

            var mostRecentRelease = Collection(queryContext).FindSync(FilterDefinition<TidyData>.Empty, options).FirstOrDefault();

            return mostRecentRelease.ReleaseId;
        }
    }
}