using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class SubjectFootnote
    {
        public Subject Subject { get; set; }
        public Guid SubjectId { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}