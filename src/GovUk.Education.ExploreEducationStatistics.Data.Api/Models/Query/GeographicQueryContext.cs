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
                queryable = queryable.Where(x => SchoolTypes.First().ToString() == x.SchoolType);
            }

            if (Years.Count > 0)
            {
                queryable = queryable.Where(x => Years.First() == x.Year);
            }

            return queryable;
        }

//
//        private ICollection<int> ShortYears()
//        {
//            return Years.Select(ShortYear()).ToList();
//        }
//
//        /**
//         * Format an academic year in the format YYYYYY e.g. 201920 as the academic start year e.g. 2019
//         */
//        private static Func<int, int> ShortYear()
//        {
//            return year => year > 10000 ? int.Parse(year.ToString().Substring(0, 4)) : year;
//        }
    }
}