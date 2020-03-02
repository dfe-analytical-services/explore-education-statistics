using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroup
    {
        public Guid Id { get; set; }
        public Filter Filter { get; set; }
        public Guid FilterId { get; set; }
        public string Label { get; set; }
        public ICollection<FilterItem> FilterItems { get; set; }
        public ICollection<FilterGroupFootnote> Footnotes { get; set; }

        public FilterGroup()
        {
        }

        public FilterGroup(Filter filter, string label)
        {
            Id = Guid.NewGuid();
            Filter = filter;
            Label = label;
            FilterItems = new List<FilterItem>();
        }
    }
}