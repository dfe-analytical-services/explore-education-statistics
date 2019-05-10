using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class SubjectMetaQueryContext
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<int> Years { get; set; }
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> LocalAuthorities { get; set; }
        public IEnumerable<string> LocalAuthorityDistricts { get; set; }
        public IEnumerable<string> Regions { get; set; }
    }
}