using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class GeographicQueryContext : IQueryContext<GeographicData>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Attributes { get; set; }

        public Expression<Func<GeographicData, bool>> FindExpression()
        {
            return x =>
                x.Level.ToLower() == Levels.getStringFromEnum(Level).ToLower() &&
                (SchoolTypes.Count == 0 ||
                 SchoolTypes.Select(Query.SchoolTypes.getStringFromEnum).Contains(x.SchoolType)) &&
                (Years.Count == 0 || Years.Contains(x.Year));
        }
    }
}