using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroupFootnote
    {
        public FilterGroup FilterGroup { get; set; }
        public Guid FilterGroupId { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}