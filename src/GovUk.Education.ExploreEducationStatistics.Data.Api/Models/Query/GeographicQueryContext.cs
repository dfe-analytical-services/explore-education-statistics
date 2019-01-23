using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class GeographicQueryContext : IQueryContext<TidyDataGeographic>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public SchoolType SchoolType { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Attributes { get; set; }

        public Expression<Func<TidyDataGeographic, bool>> FindExpression()
        {
            return x =>
                x.Level.ToLower() == Levels.getLevel(Level).ToLower() &&
                x.SchoolType == SchoolType.ToString() &&
                (Years.Count == 0 || Years.Contains(x.Year));
        }
    }
}