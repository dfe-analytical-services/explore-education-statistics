using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class ObservationQueryContext
    {
        public long SubjectId { get; set; }
        public TimePeriodQuery TimePeriod { get; set; }
        public IEnumerable<long> Filters { get; set; }
        public GeographicLevel? GeographicLevel { get; set; }
        public IEnumerable<string> Indicators { get; set; }
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

        public SubjectMetaQueryContext ToSubjectMetaQueryContext()
        {
            return new SubjectMetaQueryContext
            {
                SubjectId = SubjectId,
                TimePeriod = TimePeriod,
                GeographicLevel = GeographicLevel,
                Indicators = Indicators,
                Country = Country,
                Institution = Institution,
                LocalAuthority = LocalAuthority,
                LocalAuthorityDistrict = LocalAuthorityDistrict,
                LocalEnterprisePartnership = LocalEnterprisePartnership,
                MultiAcademyTrust = MultiAcademyTrust,
                MayoralCombinedAuthority = MayoralCombinedAuthority,
                OpportunityArea = OpportunityArea,
                ParliamentaryConstituency = ParliamentaryConstituency,
                Region = Region,
                RscRegion = RscRegion,
                Sponsor = Sponsor,
                Ward = Ward
            };
        }
    }
}