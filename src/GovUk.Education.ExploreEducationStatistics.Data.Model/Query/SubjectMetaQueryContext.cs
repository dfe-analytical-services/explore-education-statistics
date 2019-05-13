using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Query
{
    public class SubjectMetaQueryContext
    {
        public long SubjectId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<int> Years { get; set; }
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> LocalAuthorities { get; set; }
        public IEnumerable<string> LocalAuthorityDistricts { get; set; }
        public IEnumerable<string> Regions { get; set; }

        public Expression<Func<Observation, bool>> ObservationPredicate()
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(observation => observation.SubjectId == SubjectId);

            var yearsRange = TimePeriodUtil.YearsRange(Years, StartYear, EndYear);
            if (yearsRange.Any())
            {
                predicate = predicate.And(observation =>
                    yearsRange.Contains(observation.Year));
            }

            if (Countries != null && Countries.Any())
            {
                predicate = predicate.And(observation =>
                    Countries.Contains(observation.Location.Country.Code));
            }

            if (Regions != null && Regions.Any())
            {
                predicate = predicate.And(observation =>
                    Regions.Contains(observation.Location.Region.Code));
            }

            if (LocalAuthorities != null && LocalAuthorities.Any())
            {
                predicate = predicate.And(observation =>
                    LocalAuthorities.Contains(observation.Location.LocalAuthority.Code));
            }

            if (LocalAuthorityDistricts != null && LocalAuthorityDistricts.Any())
            {
                predicate = predicate.And(observation =>
                    LocalAuthorityDistricts.Contains(observation.Location.LocalAuthorityDistrict.Code));
            }

            return predicate;
        }
    }
}