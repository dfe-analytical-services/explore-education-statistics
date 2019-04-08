using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Meta
{
    public class LevelMeta
    {
        public IEnumerable<Country> Country { get; set; }
        public IEnumerable<LocalAuthority> LocalAuthority { get; set; }
        public IEnumerable<Region> Region { get; set; }
    }
}