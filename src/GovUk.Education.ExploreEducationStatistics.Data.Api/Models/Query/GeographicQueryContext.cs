using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class GeographicQueryContext : IQueryContext<GeographicData>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Attributes { get; set; }

        public IMongoQueryable<GeographicData> FindExpression(IMongoQueryable<GeographicData> queryable)
        {
            queryable = queryable.Where(x => x.Level == Level.ToString());

            if (SchoolTypes.Count > 0)
            {
                queryable = queryable.Where(x =>
                    SchoolTypes.Select(Query.SchoolTypes.EnumToString).Contains(x.SchoolType));
            }

            if (Years.Count > 0)
            {
                queryable = queryable.Where(x => Years.Contains(x.Year));
            }

            return queryable;
        }
    }
}