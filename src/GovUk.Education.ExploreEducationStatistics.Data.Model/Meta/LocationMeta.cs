using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Meta
{
    public class LocationMeta
    {
        public IEnumerable<Country> Country { get; set; }
        public IEnumerable<LocalAuthority> LocalAuthority { get; set; }
        public IEnumerable<LocalAuthorityDistrict> LocalAuthorityDistrict { get; set; }
        public IEnumerable<Region> Region { get; set; }
    }
}