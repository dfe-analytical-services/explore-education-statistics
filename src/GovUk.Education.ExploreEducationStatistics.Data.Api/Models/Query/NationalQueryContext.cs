using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class NationalQueryContext : IQueryContext<CharacteristicDataNational>
    {
        public Guid PublicationId { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Attributes { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public Expression<Func<CharacteristicDataNational, bool>> FindExpression(long releaseId)
        {
            return PredicateBuilder.True<CharacteristicDataNational>()
                .And(QueryContextUtil.ReleaseExpression<CharacteristicDataNational>(releaseId))
                .And(QueryContextUtil.LevelExpression<CharacteristicDataNational>(Level.National))
                .And(QueryContextUtil.SchoolTypeExpression<CharacteristicDataNational>(SchoolTypes))
                .And(QueryContextUtil.YearExpression<CharacteristicDataNational>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.CharacteristicsQuery<CharacteristicDataNational>(Characteristics));
        }
    }
}