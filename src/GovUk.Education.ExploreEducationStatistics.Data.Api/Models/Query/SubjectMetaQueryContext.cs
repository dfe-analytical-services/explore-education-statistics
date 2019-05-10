using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class SubjectMetaQueryContext
    {
        public long SubjectId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public IEnumerable<int> Years { get; set; }
    }
}