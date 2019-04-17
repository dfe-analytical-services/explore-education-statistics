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
        public Guid PublicationId { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public ICollection<int> Years { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Regions { get; set; }
        public ICollection<string> LocalAuthorities { get; set; }
        public ICollection<string> LocalAuthorityDistricts { get; set; }
        public ICollection<long> Indicators { get; set; }

        public Expression<Func<Observation, bool>> FindExpression(long releaseId)
        {
            return PredicateBuilder.True<Observation>()
                .And(QueryContextUtil.ReleaseExpression<Observation>(releaseId))
                .And(QueryContextUtil.GeographicLevelExpression<Observation>(GeographicLevel))
                .And(QueryContextUtil.TimePeriodExpression<Observation>(
                    QueryUtil.YearsQuery(Years, StartYear, EndYear)))
                .And(QueryContextUtil.RegionsExpression<Observation>(Regions))
                .And(QueryContextUtil.LocalAuthoritiesExpression<Observation>(LocalAuthorities))
                .And(QueryContextUtil.LocalAuthorityDistrictsExpression<Observation>(LocalAuthorityDistricts));
        }
    }
}