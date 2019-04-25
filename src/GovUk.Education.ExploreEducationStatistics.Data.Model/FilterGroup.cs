using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroup
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public Filter Filter { get; set; }
        public long FilterId { get; set; }
        public IEnumerable<FilterItem> FilterItems { get; set; }
    }
}