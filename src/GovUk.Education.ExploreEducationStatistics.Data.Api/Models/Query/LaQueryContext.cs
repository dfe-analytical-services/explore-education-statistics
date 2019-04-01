using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class LaQueryContext : IQueryContext<CharacteristicData>
    {
        public Guid PublicationId { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Regions { get; set; }
        public ICollection<string> LocalAuthorities { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Indicators { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public Expression<Func<CharacteristicData, bool>> FindExpression(long releaseId)
        {
            return PredicateBuilder.True<CharacteristicData>()
                .And(QueryContextUtil.ReleaseExpression<CharacteristicData>(releaseId))
                .And(QueryContextUtil.LevelExpression<CharacteristicData>(Level.Local_Authority))
                .And(QueryContextUtil.SchoolTypeExpression<CharacteristicData>(SchoolTypes))
                .And(QueryContextUtil.TimePeriodExpression<CharacteristicData>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.RegionsExpression<CharacteristicData>(Regions))
                .And(QueryContextUtil.LocalAuthoritiesExpression<CharacteristicData>(LocalAuthorities))
                .And(QueryContextUtil.CharacteristicsQuery<CharacteristicData>(Characteristics));
        }
    }
}