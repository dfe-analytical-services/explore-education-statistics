using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroup
    {
        public long Id { get; set; }
        public Filter Filter { get; set; }
        public long FilterId { get; set; }
        public string Label { get; set; }
        public ICollection<FilterItem> FilterItems { get; set; }

        private FilterGroup()
        {
        }

        public FilterGroup(Filter filter, string label)
        {
            Filter = filter;
            Label = label;
            FilterItems = new List<FilterItem>();
        }
    }
}