using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Location
    {
        public long Id { get; set; }
        public Country Country { get; set; }
        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public LocalAuthorityDistrict LocalAuthorityDistrict { get; set; }
        public IEnumerable<Observation> Observations { get; set; }
    }
}