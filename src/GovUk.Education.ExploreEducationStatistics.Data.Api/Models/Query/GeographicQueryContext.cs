using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;

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
        public ICollection<string> Attributes { get; set; }

        public Expression<Func<GeographicData, bool>> FindExpression()
        {
            return PredicateBuilder.True<GeographicData>()
                .And(QueryContextUtil.PublicationIdExpression<GeographicData>(PublicationId))
                .And(QueryContextUtil.LevelExpression<GeographicData>(Level))
                .And(QueryContextUtil.SchoolTypeExpression<GeographicData>(SchoolTypes))
                .And(QueryContextUtil.YearExpression<GeographicData>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(RegionsExpression())
                .And(LocalAuthoritiesExpression())
                .And(SchoolsExpression());
        }

        private Expression<Func<GeographicData, bool>> RegionsExpression()
        {
            return x => Regions == null || !Regions.Any() || Regions.Contains(x.RegionCode);
        }

        private Expression<Func<GeographicData, bool>> LocalAuthoritiesExpression()
        {
            return x => LocalAuthorities == null || !LocalAuthorities.Any() || LocalAuthorities.Contains(x.LocalAuthorityCode);
        }

        private Expression<Func<GeographicData, bool>> SchoolsExpression()
        {
            return x => Schools == null || !Schools.Any() || Schools.Contains(x.SchoolLaEstab);
        }
    }
}