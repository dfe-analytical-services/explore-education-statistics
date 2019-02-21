using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class LaQueryContext : IQueryContext<CharacteristicDataLa>
    {
        public Guid PublicationId { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Attributes { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public Expression<Func<CharacteristicDataLa, bool>> FindExpression()
        {
            return PredicateBuilder.True<CharacteristicDataLa>()
                .And(QueryContextUtil.PublicationIdExpression<CharacteristicDataLa>(PublicationId))
                .And(QueryContextUtil.LevelExpression<CharacteristicDataLa>(Level.Local_Authority))
                .And(QueryContextUtil.SchoolTypeExpression<CharacteristicDataLa>(SchoolTypes))
                .And(QueryContextUtil.YearExpression<CharacteristicDataLa>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.CharacteristicsQuery<CharacteristicDataLa>(Characteristics));
        }
    }
}