using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class GeographicQueryContext : IQueryContext<GeographicData>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Regions { get; set; }
        public ICollection<string> LocalAuthorities { get; set; }
        public ICollection<string> Schools { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Indicators { get; set; }

        public Expression<Func<GeographicData, bool>> FindExpression(long releaseId)
        {
            return PredicateBuilder.True<GeographicData>()
                .And(QueryContextUtil.ReleaseExpression<GeographicData>(releaseId))
                .And(QueryContextUtil.LevelExpression<GeographicData>(Level))
                .And(QueryContextUtil.SchoolTypeExpression<GeographicData>(SchoolTypes))
                .And(QueryContextUtil.TimePeriodExpression<GeographicData>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.RegionsExpression<GeographicData>(Regions))
                .And(QueryContextUtil.LocalAuthoritiesExpression<GeographicData>(LocalAuthorities))
                .And(SchoolsExpression());
        }
        
        private Expression<Func<GeographicData, bool>> SchoolsExpression()
        {
            return x => Schools == null || !Schools.Any() || Schools.Contains(x.SchoolLaEstab);
        }
    }
}