using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Location
    {
        public long Id { get; set; }
        public Country Country { get; set; }
        public Institution Institution { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public LocalAuthorityDistrict LocalAuthorityDistrict { get; set; }
        public LocalEnterprisePartnership LocalEnterprisePartnership { get; set; }
        public Mat Mat { get; set; }
        public MayoralCombinedAuthority MayoralCombinedAuthority { get; set; }
        public OpportunityArea OpportunityArea { get; set; }
        public ParliamentaryConstituency ParliamentaryConstituency { get; set; }
        public Provider Provider { get; set; }
        public Region Region { get; set; }
        public RscRegion RscRegion { get; set; }
        public Ward Ward { get; set; }
        public ICollection<Observation> Observations { get; set; }
    }
}