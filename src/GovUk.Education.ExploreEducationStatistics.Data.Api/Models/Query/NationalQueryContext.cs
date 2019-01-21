using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class NationalQueryContext : IQueryContext<TidyDataNationalCharacteristic>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public SchoolType SchoolType { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Attributes { get; set; }

        public Expression<Func<TidyDataNationalCharacteristic, bool>> FindExpression()
        {
            return x =>
                x.Level == Level.ToString() &&
                x.SchoolType == SchoolType.ToString() &&
                (Years.Count == 0 || Years.Contains(x.Year));
        }
    }
}