using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterFootnote
    {
        public Filter Filter { get; set; }
        public Guid FilterId { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}