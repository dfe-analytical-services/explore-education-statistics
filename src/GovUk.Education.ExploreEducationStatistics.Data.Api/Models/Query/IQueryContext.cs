using System;
using System.Collections.Generic;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public interface IQueryContext<TCollection> where TCollection : ITidyData
    {
        Guid PublicationId { get; set; }
        ICollection<string> Attributes { get; set; }
        IMongoQueryable<TCollection> FindExpression(IMongoQueryable<TCollection> queryable);
    }
}