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
        public ICollection<Observation> Observations { get; set; }
    }
}