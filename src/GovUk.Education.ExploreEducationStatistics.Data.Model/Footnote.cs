using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Footnote
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public ICollection<IndicatorFootnote> Indicators { get; set; }
        public ICollection<FilterFootnote> Filters { get; set; }
        public ICollection<FilterGroupFootnote> FilterGroups { get; set; }
        public ICollection<FilterItemFootnote> FilterItems { get; set; }
        public ICollection<SubjectFootnote> Subjects { get; set; }
    }
}