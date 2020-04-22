using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Location
    {
        public Guid Id { get; set; }
        public Country Country { get; set; }
        public Institution Institution { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public LocalAuthorityDistrict LocalAuthorityDistrict { get; set; }
        public LocalEnterprisePartnership LocalEnterprisePartnership { get; set; }
        public MayoralCombinedAuthority MayoralCombinedAuthority { get; set; }
        public Mat MultiAcademyTrust { get; set; }
        public OpportunityArea OpportunityArea { get; set; }
        public ParliamentaryConstituency ParliamentaryConstituency { get; set; }
        public Region Region { get; set; }
        public RscRegion RscRegion { get; set; }
        public Sponsor Sponsor { get; set; }
        public Ward Ward { get; set; }
        public PlanningArea PlanningArea { get; set; }
        
        public ICollection<Observation> Observations { get; set; }
    }
}