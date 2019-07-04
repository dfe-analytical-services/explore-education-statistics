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
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> Institutions { get; set; }
        public IEnumerable<string> LocalAuthorities { get; set; }
        public IEnumerable<string> LocalAuthorityDistricts { get; set; }
        public IEnumerable<string> LocalEnterprisePartnerships { get; set; }
        public IEnumerable<string> Mats { get; set; }
        public IEnumerable<string> MayoralCombinedAuthorities { get; set; }
        public IEnumerable<string> OpportunityAreas { get; set; }
        public IEnumerable<string> ParliamentaryConstituencies { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public IEnumerable<string> RscRegions { get; set; }
        public IEnumerable<string> Sponsors { get; set; }
        public IEnumerable<string> Wards { get; set; }

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

            if (Countries != null && Countries.Any())
            {
                predicate = predicate.And(observation =>
                    Countries.Contains(observation.Location.Country.Code));
            }

            if (Institutions != null && Institutions.Any())
            {
                predicate = predicate.And(observation =>
                    Institutions.Contains(observation.Location.Institution.Code));
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

            if (LocalEnterprisePartnerships != null && LocalEnterprisePartnerships.Any())
            {
                predicate = predicate.And(observation =>
                    LocalEnterprisePartnerships.Contains(observation.Location.LocalEnterprisePartnership.Code));
            }

            if (Mats != null && Mats.Any())
            {
                predicate = predicate.And(observation =>
                    Mats.Contains(observation.Location.MultiAcademyTrust.Code));
            }

            if (MayoralCombinedAuthorities != null && MayoralCombinedAuthorities.Any())
            {
                predicate = predicate.And(observation =>
                    MayoralCombinedAuthorities.Contains(observation.Location.MayoralCombinedAuthority.Code));
            }

            if (OpportunityAreas != null && OpportunityAreas.Any())
            {
                predicate = predicate.And(observation =>
                    OpportunityAreas.Contains(observation.Location.OpportunityArea.Code));
            }

            if (ParliamentaryConstituencies != null && ParliamentaryConstituencies.Any())
            {
                predicate = predicate.And(observation =>
                    ParliamentaryConstituencies.Contains(observation.Location.ParliamentaryConstituency.Code));
            }

            if (Regions != null && Regions.Any())
            {
                predicate = predicate.And(observation =>
                    Regions.Contains(observation.Location.Region.Code));
            }

            if (RscRegions != null && RscRegions.Any())
            {
                predicate = predicate.And(observation =>
                    RscRegions.Contains(observation.Location.RscRegion.Code));
            }

            if (Sponsors != null && Sponsors.Any())
            {
                predicate = predicate.And(observation =>
                    Sponsors.Contains(observation.Location.Sponsor.Code));
            }
            
            if (Wards != null && Wards.Any())
            {
                predicate = predicate.And(observation =>
                    Wards.Contains(observation.Location.Ward.Code));
            }

            return predicate;
        }
    }
}