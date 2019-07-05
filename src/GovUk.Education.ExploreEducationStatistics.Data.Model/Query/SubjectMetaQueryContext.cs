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
        public GeographicLevel? GeographicLevel { get; set; }
        public IEnumerable<long> Indicators { get; set; }
        public IEnumerable<string> Country { get; set; }
        public IEnumerable<string> Institution { get; set; }
        public IEnumerable<string> LocalAuthority { get; set; }
        public IEnumerable<string> LocalAuthorityDistrict { get; set; }
        public IEnumerable<string> LocalEnterprisePartnership { get; set; }
        public IEnumerable<string> MultiAcademyTrust { get; set; }
        public IEnumerable<string> MayoralCombinedAuthority { get; set; }
        public IEnumerable<string> OpportunityArea { get; set; }
        public IEnumerable<string> ParliamentaryConstituency { get; set; }
        public IEnumerable<string> Region { get; set; }
        public IEnumerable<string> RscRegion { get; set; }
        public IEnumerable<string> Sponsor { get; set; }
        public IEnumerable<string> Ward { get; set; }

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

            if (GeographicLevel != null)
            {
                predicate = predicate.And(observation => observation.GeographicLevel == GeographicLevel);
            }

            if (Country != null && Country.Any())
            {
                predicate = predicate.And(observation =>
                    Country.Contains(observation.Location.Country.Code));
            }

            if (Institution != null && Institution.Any())
            {
                predicate = predicate.And(observation =>
                    Institution.Contains(observation.Location.Institution.Code));
            }

            if (LocalAuthority != null && LocalAuthority.Any())
            {
                predicate = predicate.And(observation =>
                    LocalAuthority.Contains(observation.Location.LocalAuthority.Code));
            }

            if (LocalAuthorityDistrict != null && LocalAuthorityDistrict.Any())
            {
                predicate = predicate.And(observation =>
                    LocalAuthorityDistrict.Contains(observation.Location.LocalAuthorityDistrict.Code));
            }

            if (LocalEnterprisePartnership != null && LocalEnterprisePartnership.Any())
            {
                predicate = predicate.And(observation =>
                    LocalEnterprisePartnership.Contains(observation.Location.LocalEnterprisePartnership.Code));
            }

            if (MultiAcademyTrust != null && MultiAcademyTrust.Any())
            {
                predicate = predicate.And(observation =>
                    MultiAcademyTrust.Contains(observation.Location.MultiAcademyTrust.Code));
            }

            if (MayoralCombinedAuthority != null && MayoralCombinedAuthority.Any())
            {
                predicate = predicate.And(observation =>
                    MayoralCombinedAuthority.Contains(observation.Location.MayoralCombinedAuthority.Code));
            }

            if (OpportunityArea != null && OpportunityArea.Any())
            {
                predicate = predicate.And(observation =>
                    OpportunityArea.Contains(observation.Location.OpportunityArea.Code));
            }

            if (ParliamentaryConstituency != null && ParliamentaryConstituency.Any())
            {
                predicate = predicate.And(observation =>
                    ParliamentaryConstituency.Contains(observation.Location.ParliamentaryConstituency.Code));
            }

            if (Region != null && Region.Any())
            {
                predicate = predicate.And(observation =>
                    Region.Contains(observation.Location.Region.Code));
            }

            if (RscRegion != null && RscRegion.Any())
            {
                predicate = predicate.And(observation =>
                    RscRegion.Contains(observation.Location.RscRegion.Code));
            }

            if (Sponsor != null && Sponsor.Any())
            {
                predicate = predicate.And(observation =>
                    Sponsor.Contains(observation.Location.Sponsor.Code));
            }

            if (Ward != null && Ward.Any())
            {
                predicate = predicate.And(observation =>
                    Ward.Contains(observation.Location.Ward.Code));
            }

            return predicate;
        }
    }
}