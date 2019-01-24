using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class LaQueryContext : IQueryContext<CharacteristicDataLa>
    {
        public Guid PublicationId { get; set; }
        public SchoolType SchoolType { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Attributes { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public Expression<Func<CharacteristicDataLa, bool>> FindExpression()
        {
            return x =>
                x.Level.ToLower() == Levels.getLevel(Level.Local_Authority).ToLower() &&
                x.SchoolType == SchoolType.ToString() &&
                (Years.Count == 0 || Years.Contains(x.Year)) &&
                (Characteristics.Count == 0 || Characteristics.Contains(x.Characteristic.Name));
        }
    }
}