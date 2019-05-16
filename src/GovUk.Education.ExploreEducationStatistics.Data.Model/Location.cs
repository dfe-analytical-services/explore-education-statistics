using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Location
    {
        public long Id { get; set; }
        public Country Country { get; set; } = Country.Empty();
        public Region Region { get; set; } = Region.Empty();
        public LocalAuthority LocalAuthority { get; set; } = LocalAuthority.Empty();
        public LocalAuthorityDistrict LocalAuthorityDistrict { get; set; } = LocalAuthorityDistrict.Empty();
        public LocalEnterprisePartnership LocalEnterprisePartnership { get; set; } = LocalEnterprisePartnership.Empty();
        public Institution Institution { get; set; } = Institution.Empty();
        public Mat Mat { get; set; } = Mat.Empty();
        public MayoralCombinedAuthority MayoralCombinedAuthority { get; set; } = MayoralCombinedAuthority.Empty();
        public OpportunityArea OpportunityArea { get; set; } = OpportunityArea.Empty();
        public ParliamentaryConstituency ParliamentaryConstituency { get; set; } = ParliamentaryConstituency.Empty();
        public Provider Provider { get; set; } = Provider.Empty();
        public Ward Ward { get; set; } = Ward.Empty();
        public ICollection<Observation> Observations { get; set; }
    }
}