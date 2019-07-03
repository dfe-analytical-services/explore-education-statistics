using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Query
{
    public class ObservationQueryContext
    {
        public long SubjectId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<int> Years { get; set; }
        public IEnumerable<long> Filters { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
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

        public SubjectMetaQueryContext ToSubjectMetaQueryContext()
        {
            return new SubjectMetaQueryContext
            {
                SubjectId = SubjectId,
                StartYear = StartYear,
                EndYear = EndYear,
                Years = Years,
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