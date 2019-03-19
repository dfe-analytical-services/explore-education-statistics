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
        public ICollection<string> Regions { get; set; }
        public ICollection<string> LocalAuthorities { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Indicators { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public Expression<Func<CharacteristicDataLa, bool>> FindExpression(long releaseId)
        {
            return PredicateBuilder.True<CharacteristicDataLa>()
                .And(QueryContextUtil.ReleaseExpression<CharacteristicDataLa>(releaseId))
                .And(QueryContextUtil.LevelExpression<CharacteristicDataLa>(Level.Local_Authority))
                .And(QueryContextUtil.SchoolTypeExpression<CharacteristicDataLa>(SchoolTypes))
                .And(QueryContextUtil.TimePeriodExpression<CharacteristicDataLa>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.RegionsExpression<CharacteristicDataLa>(Regions))
                .And(QueryContextUtil.LocalAuthoritiesExpression<CharacteristicDataLa>(LocalAuthorities))
                .And(QueryContextUtil.CharacteristicsQuery<CharacteristicDataLa>(Characteristics));
        }
    }
}