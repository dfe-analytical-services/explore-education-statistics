using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class ObservationQueryContext
    {
        public long SubjectId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<long> Filters { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public IEnumerable<long> Indicators { get; set; }
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> LocalAuthorities { get; set; }
        public IEnumerable<string> LocalAuthorityDistricts { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public IEnumerable<int> Years { get; set; }
        public IEnumerable<string> LocalEnterprisePartnerships { get; set; }
        public IEnumerable<string> Institutions { get; set; }
        public IEnumerable<string> Mats { get; set; }
        public IEnumerable<string> MayoralCombinedAuthorities { get; set; }
        public IEnumerable<string> OpportunityAreas { get; set; }
        public IEnumerable<string> ParliamentaryConstituencies { get; set; }
        public IEnumerable<string> Providers { get; set; }
        public IEnumerable<string> Wards { get; set; }
    }
}