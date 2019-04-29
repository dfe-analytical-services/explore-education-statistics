using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class ObservationQueryContext : IQueryContext<Observation>
    {
        public long SubjectId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<long> Filters { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public IEnumerable<long> Indicators { get; set; }
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> LocalAuthorities { get; set; }
        public IEnumerable<string> LocalAuthorityDistricts { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public IEnumerable<int> Years { get; set; }

        public Expression<Func<Observation, bool>> FindExpression()
        {
            return PredicateBuilder.True<Observation>()
                .And(QueryContextUtil.SubjectExpression<Observation>(SubjectId))
                .And(QueryContextUtil.GeographicLevelExpression<Observation>(GeographicLevel))
                .And(QueryContextUtil.TimePeriodExpression<Observation>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.CountriesExpression<Observation>(Countries))
                .And(QueryContextUtil.RegionsExpression<Observation>(Regions))
                .And(QueryContextUtil.LocalAuthoritiesExpression<Observation>(LocalAuthorities))
                .And(QueryContextUtil.LocalAuthorityDistrictsExpression<Observation>(LocalAuthorityDistricts));
        }
    }
}