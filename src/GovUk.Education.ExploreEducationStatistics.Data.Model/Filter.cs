using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Filter
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Hint { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public IEnumerable<FilterGroup> FilterGroups { get; set; }
    }
}