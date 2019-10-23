using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Footnote
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public IEnumerable<IndicatorFootnote> Indicators { get; set; }
        public IEnumerable<FilterFootnote> Filters { get; set; }
        public IEnumerable<FilterGroupFootnote> FilterGroups { get; set; }
        public IEnumerable<FilterItemFootnote> FilterItems { get; set; }
        public IEnumerable<SubjectFootnote> Subjects { get; set; }
    }
}